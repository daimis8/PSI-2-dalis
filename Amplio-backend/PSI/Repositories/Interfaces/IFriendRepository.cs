using PSI.Models;

namespace PSI.Repositories.Interfaces
{
    public interface IFriendRepository
    {
        Task<FriendRequest?> GetByIdAsync(Guid id);
        Task<FriendRequest?> GetPendingAsync(Guid senderId, Guid receiverId);
        Task<List<FriendRequest>> GetIncomingPendingAsync(Guid userId);
        Task<List<FriendRequest>> GetOutgoingPendingAsync(Guid userId);
        Task AddRequestAsync(FriendRequest request);
        Task UpdateRequestAsync(FriendRequest request);
        Task<int> CountRequestsSinceAsync(Guid senderId, DateTime since);

        Task<List<Friendship>> GetFriendshipsAsync(Guid userId);
        Task<Friendship?> GetFriendshipAsync(Guid a, Guid b);
        Task AddFriendshipAsync(Friendship friendship);
        Task RemoveFriendshipAsync(Friendship friendship);

        Task<bool> BlockExistsAsync(Guid a, Guid b);
        Task<bool> IsBlockedByAsync(Guid blocker, Guid blocked);
        Task<Block?> GetBlockAsync(Guid blockerId, Guid blockedId);
        Task AddBlockAsync(Block block);
        Task RemoveBlockAsync(Block block);

        Task<User?> GetUserByIdAsync(Guid id);
        Task<List<User>> SearchUsersAsync(string query, Guid currentUserId);
    }
}
