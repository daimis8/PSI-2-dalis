using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Models;
using PSI.Repositories.Interfaces;

namespace PSI.Repositories
{
    public class SongRepository : ISongRepository
    {
        private readonly AppDbContext _context;
        
        public SongRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Song>> GetAllAsync() => await _context.Songs.ToListAsync();
        
        public async Task<Song?> GetByIdAsync(Guid id) => await _context.Songs.FindAsync(id);
        
        public async Task AddRangeAsync(IEnumerable<Song> songs)
        {
            _context.Songs.AddRange(songs);
            await _context.SaveChangesAsync();
        }
        
        public async Task RemoveAllAsync()
        {
            var songs = await _context.Songs.ToListAsync();
            _context.Songs.RemoveRange(songs);
            await _context.SaveChangesAsync();
        }
    }
}
