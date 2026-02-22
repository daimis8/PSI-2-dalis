using PSI.DTOs;

namespace PSI.Services.Interfaces
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResponseDto<PlaylistLeaderboardDto>> GetPlaylistLeaderboardAsync(int topN = 10);
        Task<LeaderboardResponseDto<AlbumLeaderboardDto>> GetAlbumLeaderboardAsync(int topN = 10);
    }
}
