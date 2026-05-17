using PSI.Models;

namespace PSI.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<Playlist?> GetByIdAsync(Guid id);
        Task<Playlist?> GetDetailedByIdAsync(Guid id);
        Task AddAsync(Playlist playlist);
        Task UpdateAsync(Playlist playlist);
        Task<List<Playlist>> GetPublicPlaylistsAsync();
        Task<List<Playlist>> GetAllAsync();
        Task ClearCurrentSongForAllAsync();

        Task<List<Playlist>> GetInvitedPlaylistsAsync(Guid userId);
        Task<bool> InvitationExistsAsync(Guid playlistId, Guid inviteeId);
        Task AddInvitationAsync(PlaylistInvitation invitation);
    }
}
