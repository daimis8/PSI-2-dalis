using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Models;
using PSI.Repositories.Interfaces;

namespace PSI.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly AppDbContext _context;
        
        public AlbumRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Album>> GetAllAsync() => await _context.Albums.AsNoTracking().ToListAsync();

        public async Task<Album?> GetByIdAsync(Guid id) => await _context.Albums.Include(a => a.Songs).FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Album?> GetByNameAndArtistAsync(string name, string artist)
        {
            return await _context.Albums.FirstOrDefaultAsync(a => a.Name == name && a.Artist == artist);
        }

        public async Task AddAsync(Album album)
        {
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
        }
        
        public async Task UpdateAsync(Album album)
        {
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<Album>> GetAllWithSongsAsync()
        {
            return await _context.Albums.Include(a => a.Songs).ToListAsync();
        }
        
        public async Task RemoveAllAsync()
        {
            var albums = await _context.Albums.ToListAsync();
            _context.Albums.RemoveRange(albums);
            await _context.SaveChangesAsync();
        }
    }
}
