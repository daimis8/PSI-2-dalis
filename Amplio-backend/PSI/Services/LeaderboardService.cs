using PSI.DTOs;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services.Interfaces;

namespace PSI.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IAlbumRepository _albumRepository;

        public LeaderboardService(IPlaylistRepository playlistRepository, IAlbumRepository albumRepository)
        {
            _playlistRepository = playlistRepository;
            _albumRepository = albumRepository;
        }
        public async Task<LeaderboardResponseDto<PlaylistLeaderboardDto>> GetPlaylistLeaderboardAsync(int topN = 10)
        {
            var leaderboard = new GenericLeaderboard<Playlist>();

            var playlists = await _playlistRepository.GetPublicPlaylistsAsync();

            foreach (var playlist in playlists)
            {
                leaderboard.AddSongCollection(playlist);
            }

            var sortedPlaylists = leaderboard.GetSortedByPopularity()
                .Take(topN)
                .Select(p => new PlaylistLeaderboardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Popularity = p.Popularity,
                    IsPublic = p.IsPublic
                })
                .ToList();

            return new LeaderboardResponseDto<PlaylistLeaderboardDto>
            {
                LeaderboardItems = sortedPlaylists
            };
        }

        public async Task<LeaderboardResponseDto<AlbumLeaderboardDto>> GetAlbumLeaderboardAsync(int topN = 10)
        {
            var leaderboard = new GenericLeaderboard<Album>();

            var albums = await _albumRepository.GetAllAsync();

            foreach (var album in albums)
            {
                leaderboard.AddSongCollection(album);
            }

            var sortedAlbums = leaderboard.GetSortedByPopularity()
                .Take(topN)
                .Select(a => new AlbumLeaderboardDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Artist = a.Artist,
                    ReleaseYear = a.ReleaseYear,
                    Popularity = a.Popularity
                })
                .ToList();

            return new LeaderboardResponseDto<AlbumLeaderboardDto>
            {
                LeaderboardItems = sortedAlbums
            };
        }
    }
}
