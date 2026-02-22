using PSI.DTOs;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PSI.Services
{
    public class SongService : ISongService
    {
        private readonly ISongRepository _songRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistSongRepository _playlistSongRepository;

        public SongService(
            ISongRepository songRepository,
            IAlbumRepository albumRepository,
            IPlaylistRepository playlistRepository,
            IPlaylistSongRepository playlistSongRepository)
        {
            _songRepository = songRepository;
            _albumRepository = albumRepository;
            _playlistRepository = playlistRepository;
            _playlistSongRepository = playlistSongRepository;
        }

        public async Task<IEnumerable<Song>> GetAllSongsAsync()
        {
            return await _songRepository.GetAllAsync();
        }

        public async Task<Song?> GetSongByIdAsync(Guid songId)
        {
            return await _songRepository.GetByIdAsync(songId);
        }

        public async Task<List<Song>> ImportSongsFromFileAsync()
        {
            await ClearExistingDataAsync();

            List<SongDto>? songDtos = await LoadSongsFromFileAsync();
            if (songDtos == null)
                throw new InvalidOperationException("Failed to deserialize songs from file");

            var songs = new List<Song>();
            var albums = new Dictionary<string, Album>(); // Track albums by key

            foreach (var dto in songDtos)
            {
                Album? albumEntity = null;

                if (dto.Album != null)
                {
                    string albumKey = $"{dto.Album.Name}|{dto.Album.Artist}";

                    if (!albums.TryGetValue(albumKey, out albumEntity))
                    {
                        albumEntity = await _albumRepository.GetByNameAndArtistAsync(dto.Album.Name, dto.Album.Artist);

                        if (albumEntity == null)
                        {
                            albumEntity = new Album(dto.Album.Name, dto.Album.Artist, dto.Album.ReleaseYear);
                            await _albumRepository.AddAsync(albumEntity);
                        }
                        albums[albumKey] = albumEntity;
                    }
                }

                var song = new Song
                {
                    Title = dto.Title,
                    Artist = dto.Artist,
                    Link = new SongLink(dto.Link),
                    Genres = dto.Genres.ToList(),
                    Album = albumEntity,
                    AlbumId = albumEntity?.Id
                };
                songs.Add(song);
            }

            await _songRepository.AddRangeAsync(songs);
            return songs;
        }

        private async Task ClearExistingDataAsync()
        {
            await _playlistRepository.ClearCurrentSongForAllAsync();
            await _playlistSongRepository.RemoveAllAsync();
            await _songRepository.RemoveAllAsync();
            await _albumRepository.RemoveAllAsync();
        }

        private async Task<List<SongDto>?> LoadSongsFromFileAsync()
        {
            string filePath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "DummyData",
                path3: "songs.json"
            );

            using Stream fileStream = File.OpenRead(filePath);

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());

            var songDtos = await JsonSerializer.DeserializeAsync<List<SongDto>>(fileStream, options);
            return songDtos;
        }
    }
}
