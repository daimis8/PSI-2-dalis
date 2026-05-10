using PSI.DTOs;

namespace PSI.Services.Interfaces
{
    public interface IFriendService
    {
        Task<FriendRequestDto> SendRequestAsync(Guid senderId, Guid receiverId);
        Task CancelRequestAsync(Guid senderId, Guid requestId);
        Task<FriendDto> AcceptRequestAsync(Guid receiverId, Guid requestId);
        Task DeclineRequestAsync(Guid receiverId, Guid requestId);
        Task RemoveFriendAsync(Guid userId, Guid friendId);
        Task BlockUserAsync(Guid blockerId, Guid targetId);
        Task UnblockUserAsync(Guid blockerId, Guid targetId);

        Task<List<FriendDto>> GetFriendsAsync(Guid userId);
        Task<List<FriendRequestDto>> GetIncomingRequestsAsync(Guid userId);
        Task<List<FriendRequestDto>> GetOutgoingRequestsAsync(Guid userId);
        Task<List<UserSearchResultDto>> SearchUsersAsync(Guid currentUserId, string query);

        Task<bool> AreFriendsAsync(Guid userA, Guid userB);
    }
}
