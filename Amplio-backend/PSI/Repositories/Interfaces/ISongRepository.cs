using PSI.Models;

namespace PSI.Repositories.Interfaces
{
    public interface ISongRepository
    {
        Task<List<Song>> GetAllAsync();
        Task<Song?> GetByIdAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Song> songs);
        Task RemoveAllAsync();
    }
}
