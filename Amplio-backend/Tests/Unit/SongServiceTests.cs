using FluentAssertions;
using Moq;
using PSI.DTOs;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services;

namespace Tests.Unit;

public class SongServiceTests
{
    private readonly Mock<ISongRepository> _songRepository;
    private readonly Mock<IAlbumRepository> _albumRepository;
    private readonly Mock<IPlaylistRepository> _playlistRepository;
    private readonly Mock<IPlaylistSongRepository> _playlistSongRepository;

    private readonly SongService _songService;

    public SongServiceTests()
    {
        _songRepository = new Mock<ISongRepository>();
        _albumRepository = new Mock<IAlbumRepository>();
        _playlistRepository = new Mock<IPlaylistRepository>();
        _playlistSongRepository = new Mock<IPlaylistSongRepository>();

        _songService = new SongService(
            _songRepository.Object,
            _albumRepository.Object,
            _playlistRepository.Object,
            _playlistSongRepository.Object
        );
    }

    private string EnsureDummyDataFile()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "DummyData");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "songs.json");
        var data = "[" +
                   "{\n" +
                   "  \"Title\": \"Song One\",\n" +
                   "  \"Artist\": \"Artist A\",\n" +
                   "  \"Genres\": [\"Rock\"],\n" +
                   "  \"Link\": \"https://youtu.be/VIDEO1\",\n" +
                   "  \"Album\": { \"Name\": \"AlbumX\", \"Artist\": \"Artist A\", \"ReleaseYear\": 2001 }\n" +
                   "}," +
                   "{\n" +
                   "  \"Title\": \"Song Two\",\n" +
                   "  \"Artist\": \"Artist A\",\n" +
                   "  \"Genres\": [\"Pop\"],\n" +
                   "  \"Link\": \"https://www.youtube.com/watch?v=VIDEO2\",\n" +
                   "  \"Album\": { \"Name\": \"AlbumX\", \"Artist\": \"Artist A\", \"ReleaseYear\": 2001 }\n" +
                   "}" +
                   "]";
        File.WriteAllText(file, data);
        return file;
    }

    [Fact]
    public async Task ImportSongsFromFileAsync_LoadsSongs_And_DeduplicatesAlbum()
    {
        EnsureDummyDataFile();
        _albumRepository.Setup(a => a.GetByNameAndArtistAsync("AlbumX", "Artist A")).ReturnsAsync((Album?)null);

        var songs = await _songService.ImportSongsFromFileAsync();

        songs.Should().HaveCount(2);
        _albumRepository.Verify(a => a.AddAsync(It.IsAny<Album>()), Times.Once);
        _songRepository.Verify(s => s.AddRangeAsync(It.IsAny<IEnumerable<Song>>()), Times.Once);
        _playlistRepository.Verify(p => p.ClearCurrentSongForAllAsync(), Times.Once);
        _playlistSongRepository.Verify(ps => ps.RemoveAllAsync(), Times.Once);
        _songRepository.Verify(s => s.RemoveAllAsync(), Times.Once);
        _albumRepository.Verify(a => a.RemoveAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllSongsAsync_ReturnsRepositoryResult()
    {
        var expected = new List<Song> { new Song { Title = "T" } };
        _songRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);
        var result = await _songService.GetAllSongsAsync();
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetSongByIdAsync_ReturnsRepositoryResult()
    {
        var id = Guid.NewGuid();
        var song = new Song { Id = id, Title = "X" };
        _songRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(song);
        var result = await _songService.GetSongByIdAsync(id);
        result.Should().Be(song);
    }
}
