using PSI.Models;

namespace PSI.Repositories.Interfaces
{
    public interface IPlaylistSongRepository
    {
        Task RemoveAllAsync();
    }
}
