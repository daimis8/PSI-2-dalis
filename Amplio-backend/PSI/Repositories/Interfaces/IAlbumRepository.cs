using PSI.Models;

namespace PSI.Repositories.Interfaces
{
    public interface IAlbumRepository
    {
        Task<List<Album>> GetAllAsync();
        Task<Album?> GetByIdAsync(Guid id);
        Task<Album?> GetByNameAndArtistAsync(string name, string artist);
        Task AddAsync(Album album);
        Task UpdateAsync(Album album);
        Task<List<Album>> GetAllWithSongsAsync();
        Task RemoveAllAsync();
    }
}
