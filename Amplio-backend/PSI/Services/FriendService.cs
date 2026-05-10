using PSI.DTOs;
using PSI.Exceptions;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services.Interfaces;

namespace PSI.Services
{
    public class FriendService : IFriendService
    {
        private const int RequestRateLimitPerHour = 20;

        private readonly IFriendRepository _friends;

        public FriendService(IFriendRepository friends)
        {
            _friends = friends;
        }

        public async Task<FriendRequestDto> SendRequestAsync(Guid senderId, Guid receiverId)
        {
            if (senderId == receiverId)
                throw new SelfFriendRequestException();

            var receiver = await _friends.GetUserByIdAsync(receiverId)
                ?? throw new KeyNotFoundException("Target user not found");

            if (await _friends.BlockExistsAsync(senderId, receiverId))
                throw new BlockedRelationshipException();

            if (await _friends.GetFriendshipAsync(senderId, receiverId) is not null)
                throw new AlreadyFriendsException();

            // Either-direction pending check covers both "sender already asked" and
            // "receiver already asked sender" — both should be a no-op for the new request.
            if (await _friends.GetPendingAsync(senderId, receiverId) is not null
                || await _friends.GetPendingAsync(receiverId, senderId) is not null)
                throw new DuplicateFriendRequestException();

            var since = DateTime.UtcNow.AddHours(-1);
            if (await _friends.CountRequestsSinceAsync(senderId, since) >= RequestRateLimitPerHour)
                throw new FriendRequestRateLimitException();

            var sender = await _friends.GetUserByIdAsync(senderId)
                ?? throw new KeyNotFoundException("Sender not found");

            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
                SentAt = DateTime.UtcNow
            };
            await _friends.AddRequestAsync(request);

            return new FriendRequestDto(
                request.Id,
                sender.Id, sender.Username,
                receiver.Id, receiver.Username,
                request.Status, request.SentAt);
        }

        public async Task CancelRequestAsync(Guid senderId, Guid requestId)
        {
            var request = await _friends.GetByIdAsync(requestId)
                ?? throw new FriendRequestNotFoundException();

            if (request.SenderId != senderId)
                throw new FriendRequestNotFoundException();

            if (request.Status != FriendRequestStatus.Pending)
                throw new FriendRequestNotFoundException();

            request.Status = FriendRequestStatus.Cancelled;
            await _friends.UpdateRequestAsync(request);
        }

        public async Task<FriendDto> AcceptRequestAsync(Guid receiverId, Guid requestId)
        {
            var request = await _friends.GetByIdAsync(requestId)
                ?? throw new FriendRequestNotFoundException();

            if (request.ReceiverId != receiverId)
                throw new FriendRequestNotFoundException();

            if (request.Status != FriendRequestStatus.Pending)
                throw new FriendRequestNotFoundException();

            if (await _friends.BlockExistsAsync(request.SenderId, request.ReceiverId))
                throw new BlockedRelationshipException();

            request.Status = FriendRequestStatus.Accepted;
            await _friends.UpdateRequestAsync(request);

            var friendship = new Friendship
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId,
                CreatedAt = DateTime.UtcNow
            };
            await _friends.AddFriendshipAsync(friendship);

            var sender = await _friends.GetUserByIdAsync(request.SenderId)
                ?? throw new KeyNotFoundException("Sender not found");

            return new FriendDto(sender.Id, sender.Username, friendship.CreatedAt);
        }

        public async Task DeclineRequestAsync(Guid receiverId, Guid requestId)
        {
            var request = await _friends.GetByIdAsync(requestId)
                ?? throw new FriendRequestNotFoundException();

            if (request.ReceiverId != receiverId)
                throw new FriendRequestNotFoundException();

            if (request.Status != FriendRequestStatus.Pending)
                throw new FriendRequestNotFoundException();

            request.Status = FriendRequestStatus.Declined;
            await _friends.UpdateRequestAsync(request);
        }

        public async Task RemoveFriendAsync(Guid userId, Guid friendId)
        {
            var friendship = await _friends.GetFriendshipAsync(userId, friendId)
                ?? throw new FriendshipNotFoundException();
            await _friends.RemoveFriendshipAsync(friendship);
        }

        public async Task BlockUserAsync(Guid blockerId, Guid targetId)
        {
            if (blockerId == targetId)
                throw new ArgumentException("Cannot block yourself");

            _ = await _friends.GetUserByIdAsync(targetId)
                ?? throw new KeyNotFoundException("Target user not found");

            // Idempotent: if a block already exists, do nothing.
            if (await _friends.GetBlockAsync(blockerId, targetId) is not null) return;

            // Cancel any Pending request between the two parties (either direction).
            var outgoing = await _friends.GetPendingAsync(blockerId, targetId);
            if (outgoing is not null)
            {
                outgoing.Status = FriendRequestStatus.Blocked;
                await _friends.UpdateRequestAsync(outgoing);
            }
            var incoming = await _friends.GetPendingAsync(targetId, blockerId);
            if (incoming is not null)
            {
                incoming.Status = FriendRequestStatus.Blocked;
                await _friends.UpdateRequestAsync(incoming);
            }

            // Drop any existing friendship between the parties.
            var friendship = await _friends.GetFriendshipAsync(blockerId, targetId);
            if (friendship is not null)
                await _friends.RemoveFriendshipAsync(friendship);

            await _friends.AddBlockAsync(new Block
            {
                BlockerId = blockerId,
                BlockedId = targetId,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task UnblockUserAsync(Guid blockerId, Guid targetId)
        {
            var block = await _friends.GetBlockAsync(blockerId, targetId);
            if (block is null) return;
            await _friends.RemoveBlockAsync(block);
        }

        public async Task<List<FriendDto>> GetFriendsAsync(Guid userId)
        {
            var friendships = await _friends.GetFriendshipsAsync(userId);
            return friendships.Select(f =>
            {
                var other = f.UserId == userId ? f.Friend : f.User;
                return new FriendDto(other!.Id, other.Username, f.CreatedAt);
            }).ToList();
        }

        public async Task<List<FriendRequestDto>> GetIncomingRequestsAsync(Guid userId)
        {
            var requests = await _friends.GetIncomingPendingAsync(userId);
            return requests.Select(ToDto).ToList();
        }

        public async Task<List<FriendRequestDto>> GetOutgoingRequestsAsync(Guid userId)
        {
            var requests = await _friends.GetOutgoingPendingAsync(userId);
            return requests.Select(ToDto).ToList();
        }

        public async Task<List<UserSearchResultDto>> SearchUsersAsync(Guid currentUserId, string query)
        {
            var users = await _friends.SearchUsersAsync(query, currentUserId);
            return users.Select(u => new UserSearchResultDto(u.Id, u.Username)).ToList();
        }

        public async Task<bool> AreFriendsAsync(Guid userA, Guid userB) =>
            await _friends.GetFriendshipAsync(userA, userB) is not null;

        private static FriendRequestDto ToDto(FriendRequest fr) => new(
            fr.Id,
            fr.SenderId, fr.Sender?.Username ?? string.Empty,
            fr.ReceiverId, fr.Receiver?.Username ?? string.Empty,
            fr.Status, fr.SentAt);
    }
}
