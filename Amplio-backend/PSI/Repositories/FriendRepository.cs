using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Models;
using PSI.Repositories.Interfaces;

namespace PSI.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly AppDbContext _context;

        public FriendRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<FriendRequest?> GetByIdAsync(Guid id) =>
            _context.FriendRequests.FirstOrDefaultAsync(fr => fr.Id == id);

        public Task<FriendRequest?> GetPendingAsync(Guid senderId, Guid receiverId) =>
            _context.FriendRequests.FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId &&
                fr.ReceiverId == receiverId &&
                fr.Status == FriendRequestStatus.Pending);

        public async Task<List<FriendRequest>> GetIncomingPendingAsync(Guid userId) =>
            await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .OrderByDescending(fr => fr.SentAt)
                .ToListAsync();

        public async Task<List<FriendRequest>> GetOutgoingPendingAsync(Guid userId) =>
            await _context.FriendRequests
                .Include(fr => fr.Receiver)
                .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending)
                .OrderByDescending(fr => fr.SentAt)
                .ToListAsync();

        public async Task AddRequestAsync(FriendRequest request)
        {
            _context.FriendRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequestAsync(FriendRequest request)
        {
            _context.FriendRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public Task<int> CountRequestsSinceAsync(Guid senderId, DateTime since) =>
            _context.FriendRequests.CountAsync(fr => fr.SenderId == senderId && fr.SentAt >= since);

        public async Task<List<Friendship>> GetFriendshipsAsync(Guid userId) =>
            await _context.Friendships
                .Include(f => f.User)
                .Include(f => f.Friend)
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

        public Task<Friendship?> GetFriendshipAsync(Guid a, Guid b) =>
            _context.Friendships.FirstOrDefaultAsync(f =>
                (f.UserId == a && f.FriendId == b) ||
                (f.UserId == b && f.FriendId == a));

        public async Task AddFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public Task<bool> BlockExistsAsync(Guid a, Guid b) =>
            _context.Blocks.AnyAsync(block =>
                (block.BlockerId == a && block.BlockedId == b) ||
                (block.BlockerId == b && block.BlockedId == a));

        public Task<bool> IsBlockedByAsync(Guid blocker, Guid blocked) =>
            _context.Blocks.AnyAsync(block => block.BlockerId == blocker && block.BlockedId == blocked);

        public Task<Block?> GetBlockAsync(Guid blockerId, Guid blockedId) =>
            _context.Blocks.FirstOrDefaultAsync(block =>
                block.BlockerId == blockerId && block.BlockedId == blockedId);

        public async Task AddBlockAsync(Block block)
        {
            _context.Blocks.Add(block);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBlockAsync(Block block)
        {
            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();
        }

        public Task<User?> GetUserByIdAsync(Guid id) =>
            _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

        public async Task<List<User>> SearchUsersAsync(string query, Guid currentUserId)
        {
            var trimmed = (query ?? string.Empty).Trim();
            if (trimmed.Length == 0) return new List<User>();

            var needle = trimmed.ToLower();

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id != currentUserId)
                .Where(u => u.Username.ToLower().Contains(needle))
                .Where(u => !_context.Blocks.Any(b => b.BlockerId == u.Id && b.BlockedId == currentUserId))
                .OrderBy(u => u.Username)
                .Take(50)
                .ToListAsync();
        }
    }
}
