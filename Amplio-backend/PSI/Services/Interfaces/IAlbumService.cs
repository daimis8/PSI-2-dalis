using PSI.Models;

namespace PSI.Services.Interfaces
{
    public interface IAlbumService
    {
        Task<List<Album>> GetAllAlbumsAsync();
        Task<Album?> GetAlbumByIdAsync(Guid albumId);
        Task<Album> CreateAlbumAsync(string name, string artist, int releaseYear);
        Task AddSongToAlbumAsync(Guid albumId, Guid songId);
        Task IncreaseAlbumPopularityAsync(Guid albumId);
    }
}
