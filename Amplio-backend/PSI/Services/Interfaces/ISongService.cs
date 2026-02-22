using PSI.Models;

namespace PSI.Services.Interfaces
{
    public interface ISongService
    {
        Task<IEnumerable<Song>> GetAllSongsAsync();
        Task<Song?> GetSongByIdAsync(Guid songId);
        Task<List<Song>> ImportSongsFromFileAsync();
    }
}
