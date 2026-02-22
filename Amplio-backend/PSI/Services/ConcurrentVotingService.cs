using System.Collections.Concurrent;
using PSI.Services.Interfaces;
using PSI.Repositories.Interfaces;

namespace PSI.Services
{
    public class ConcurrentVotingService : IConcurrentVotingService
    {
        private readonly IPlaylistRepository _playlistRepository;

        private readonly ConcurrentDictionary<string, int> _votesCache = new();

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public ConcurrentVotingService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public async Task UpvoteAsync(Guid playlistId, Guid songId)
        {
            string cacheKey = GetCacheKey(playlistId, songId);

            await _semaphore.WaitAsync();
            try
            {
                var playlist = await _playlistRepository.GetDetailedByIdAsync(playlistId);
                if (playlist == null)
                    throw new KeyNotFoundException("Playlist not found");

                var playlistSong = playlist.GetSongById(songId);
                if (playlistSong == null)
                    throw new KeyNotFoundException("Song not found in playlist");

                playlistSong.Upvote();

                _votesCache.AddOrUpdate(
                    cacheKey,
                    playlistSong.Votes, 
                    (key, oldValue) => playlistSong.Votes  
                );


                await _playlistRepository.UpdateAsync(playlist);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> GetVotesAsync(Guid playlistId, Guid songId)
        {
            string cacheKey = GetCacheKey(playlistId, songId);


            if (_votesCache.TryGetValue(cacheKey, out int cachedVotes))
            {
                return cachedVotes;
            }


            var playlist = await _playlistRepository.GetDetailedByIdAsync(playlistId);
            var playlistSong = playlist?.GetSongById(songId);

            if (playlistSong != null)
            {
                _votesCache.TryAdd(cacheKey, playlistSong.Votes);
                return playlistSong.Votes;
            }

            return 0;
        }

        public async Task InitializeCacheAsync()
        {
            var allPlaylists = await _playlistRepository.GetAllAsync();

            foreach (var playlist in allPlaylists)
            {
                foreach (var playlistSong in playlist.Songs)
                {
                    string cacheKey = GetCacheKey(playlist.Id, playlistSong.SongId);
                    _votesCache.TryAdd(cacheKey, playlistSong.Votes);
                }
            }
        }

        private static string GetCacheKey(Guid playlistId, Guid songId)
        {
            return $"{playlistId}:{songId}";
        }
    }
}