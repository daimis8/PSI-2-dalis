using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Repositories.Interfaces;

namespace PSI.Repositories
{
    public class PlaylistSongRepository : IPlaylistSongRepository
    {
        private readonly AppDbContext _context;

        public PlaylistSongRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task RemoveAllAsync()
        {
            var playlistSongs = await _context.PlaylistSongs.ToListAsync();
            _context.PlaylistSongs.RemoveRange(playlistSongs);
            await _context.SaveChangesAsync();
        }
    }
}
