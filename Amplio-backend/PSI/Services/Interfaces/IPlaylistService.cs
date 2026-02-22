using PSI.Models;

namespace PSI.Services.Interfaces
{
    public interface IPlaylistService
    {
        Task<Playlist> CreatePlaylistAsync(string name, bool isPublic, Guid? currentSongId, Guid ownerId);
        Task<List<PlaylistSong>> GetSongsInPlaylistAsync(Guid playlistId);
        Task<List<PlaylistSong>> AddSongToPlaylistAsync(Guid playlistId, Guid songId);
        Task RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId);
        Task IncreasePlaylistPopularityAsync(Guid playlistId);
        Task<List<PlaylistSong>> UpvoteSongInPlaylistAsync(Guid playlistId, Guid songId);
        Task<Song> SetCurrentSongAsync(Guid playlistId);
        Task<Song?> GetCurrentSongAsync(Guid playlistId);
        Task ClearCurrentSongAsync(Guid playlistId);
        Task<Playlist> GetPlaylistByIdAsync(Guid playlistId);
        Task<List<Playlist>> GetPlaylistsByOwnerAsync(Guid ownerId);
    }
}
