using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Models;
using PSI.Repositories.Interfaces;

namespace PSI.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly AppDbContext _context;

        public PlaylistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task<Playlist?> GetByIdAsync(Guid id)
        {
            return await _context.Playlists.FindAsync(id);
        }

        public async Task<Playlist?> GetDetailedByIdAsync(Guid id)
        {
            return await _context.Playlists
                .Include(p => p.CurrentSong)
                .Include(p => p.Songs)
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Playlist>> GetPublicPlaylistsAsync()
        {
            return await _context.Playlists.AsNoTracking().Where(p => p.IsPublic).ToListAsync();
        }

        public async Task<List<Playlist>> GetAllAsync()
        {
            return await _context.Playlists.ToListAsync();
        }

        public async Task ClearCurrentSongForAllAsync()
        {
            var playlists = await _context.Playlists.ToListAsync();
            playlists.ForEach(p => p.CurrentSongId = null);
            await _context.SaveChangesAsync();
        }
    }
}
