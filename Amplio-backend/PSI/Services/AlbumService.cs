using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services.Interfaces;

namespace PSI.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly ISongRepository _songRepository;

        public AlbumService(IAlbumRepository albumRepository, ISongRepository songRepository)
        {
            _albumRepository = albumRepository;
            _songRepository = songRepository;
        }
        public async Task<List<Album>> GetAllAlbumsAsync()
        {
            return await _albumRepository.GetAllWithSongsAsync();
        }

        public async Task<Album?> GetAlbumByIdAsync(Guid albumId)
        {
            return await _albumRepository.GetByIdAsync(albumId);
        }
        public async Task<Album> CreateAlbumAsync(string name, string artist, int releaseYear)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Album name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(artist))
                throw new ArgumentException("Artist name cannot be empty", nameof(artist));
            
            if (releaseYear > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException(nameof(releaseYear), "Release year is out of valid range.");

            var album = new Album(name, artist, releaseYear);

            await _albumRepository.AddAsync(album);
            return album;
        }

        public async Task AddSongToAlbumAsync(Guid albumId, Guid songId)
        {
            var album = await GetAlbumByIdAsync(albumId)
                ?? throw new KeyNotFoundException("Album not found");

            var song = await _songRepository.GetByIdAsync(songId)
                ?? throw new KeyNotFoundException("Song not found");

            album.AddSong(song);
            song.AlbumId = albumId;
            song.Album = album;

            await _albumRepository.UpdateAsync(album);
        }

        public async Task IncreaseAlbumPopularityAsync(Guid albumId)
        {
            var album = await GetAlbumByIdAsync(albumId);
            if (album != null)
            {
                album.IncreasePopularity();
                await _albumRepository.UpdateAsync(album);
            }
        }
    }
}
