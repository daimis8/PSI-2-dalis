using PSI.Models;
using PSI.Exceptions;
using PSI.Repositories.Interfaces;
using PSI.Services.Interfaces;

namespace PSI.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ISongRepository _songRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IConcurrentVotingService _votingService;

        public PlaylistService(
            IPlaylistRepository playlistRepository,
            ISongRepository songRepository,
            IAlbumRepository albumRepository,
            IConcurrentVotingService votingService)
        {
            _playlistRepository = playlistRepository;
            _songRepository = songRepository;
            _albumRepository = albumRepository;
            _votingService = votingService;
        }

        public async Task<Playlist> CreatePlaylistAsync(string name, bool isPublic, Guid? currentSongId, Guid ownerId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Playlist name cannot be empty", nameof(name));
            }

            var playlist = new Playlist(name, isPublic, ownerId)
            {
                CurrentSongId = currentSongId
            };

            await _playlistRepository.AddAsync(playlist);
            return playlist;
        }

        public async Task<List<PlaylistSong>> GetSongsInPlaylistAsync(Guid playlistId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);
            return playlist.GetAllSongs();
        }

        public async Task<List<PlaylistSong>> AddSongToPlaylistAsync(Guid playlistId, Guid songId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);
            var song = await _songRepository.GetByIdAsync(songId)
                ?? throw new KeyNotFoundException("Song not found");

            playlist.AddSong(song);
            if (song.AlbumId.HasValue)
            {
                var album = await _albumRepository.GetByIdAsync(song.AlbumId.Value);
                if (album != null)
                {
                    album.IncreasePopularity();
                    await _albumRepository.UpdateAsync(album);
                }
            }

            await _playlistRepository.UpdateAsync(playlist);
            return playlist.GetAllSongs();
        }

        public async Task RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);

            if (!playlist.DeleteSong(songId))
            {
                throw new KeyNotFoundException("Song not found in playlist");
            }

            await _playlistRepository.UpdateAsync(playlist);
        }

        public async Task IncreasePlaylistPopularityAsync(Guid playlistId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);
            if (playlist == null)
            {
                throw new KeyNotFoundException("Playlist not found");
            }

            playlist.IncreasePopularity();
            await _playlistRepository.UpdateAsync(playlist);
        }

        public async Task<List<PlaylistSong>> UpvoteSongInPlaylistAsync(Guid playlistId, Guid songId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);

            var playlistSong = playlist.GetSongById(songId);
            if (playlistSong == null)
                throw new KeyNotFoundException("Song not found in playlist");

            await _votingService.UpvoteAsync(playlistId, songId);

            playlist = await GetPlaylistByIdAsync(playlistId);
            return playlist.GetAllSongs();
        }

        public async Task<Song> SetCurrentSongAsync(Guid playlistId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);
            var playlistSongs = playlist.GetAllSongs();

            if (!playlistSongs.Any())
            {
                throw new PlaylistOperationException("Cannot set current song because the playlist is empty.", playlist.Id);
            }

            var topSong = playlistSongs.First().Song;
            playlist.CurrentSong = topSong;

            playlist.DeleteSong(topSong.Id);

            await _playlistRepository.UpdateAsync(playlist);
            return playlist.CurrentSong!;
        }

        public async Task<Song?> GetCurrentSongAsync(Guid playlistId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);
            return playlist.CurrentSong;
        }

        public async Task<Playlist> GetPlaylistByIdAsync(Guid playlistId)
        {
            var playlist = await _playlistRepository.GetDetailedByIdAsync(playlistId);
            return playlist ?? throw new KeyNotFoundException("Playlist not found");
        }

        public async Task ClearCurrentSongAsync(Guid playlistId)
        {
            var playlist = await GetPlaylistByIdAsync(playlistId);
            playlist.CurrentSong = null;
            playlist.CurrentSongId = null;
            await _playlistRepository.UpdateAsync(playlist);
        }
        public async Task<List<Playlist>> GetPlaylistsByOwnerAsync(Guid ownerId)
        {
            var allPlaylists = await _playlistRepository.GetAllAsync();

            var userPlaylists = allPlaylists
                .Where(p => p.OwnerId == ownerId)
                .ToList();

            if (!userPlaylists.Any())
            {
                throw new KeyNotFoundException("No playlists found for this user.");
            }

            return userPlaylists;
        }
    }
}
