using FluentAssertions;
using Moq;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services;

namespace Tests.Unit;

public class AlbumServiceTests
{
    private readonly Mock<IAlbumRepository> _albumRepository;
    private readonly Mock<ISongRepository> _songRepository;

    private readonly AlbumService _albumService;

    public AlbumServiceTests()
    {
        _albumRepository = new Mock<IAlbumRepository>();
        _songRepository = new Mock<ISongRepository>();
        _albumService = new AlbumService(_albumRepository.Object, _songRepository.Object);
    }

    [Fact]
    public async Task CreateAlbumAsync_ValidData_CreatesAndReturnsAlbum()
    {
        var album = await _albumService.CreateAlbumAsync("Name", "Artist", 1999);

        album.Name.Should().Be("Name");
        album.Artist.Should().Be("Artist");
        album.ReleaseYear.Should().Be(1999);
        _albumRepository.Verify(r => r.AddAsync(It.IsAny<Album>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAlbumAsync_InvalidName_Throws(string name)
    {
        var act = async () => await _albumService.CreateAlbumAsync(name, "Artist", 1999);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAlbumAsync_InvalidArtist_Throws(string artist)
    {
        var act = async () => await _albumService.CreateAlbumAsync("Name", artist, 1999);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAlbumAsync_FutureYear_Throws()
    {
        var act = async () => await _albumService.CreateAlbumAsync("Name", "Artist", DateTime.Now.Year + 1);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task AddSongToAlbumAsync_Adds_And_UpdatesRepository()
    {
        var albumId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        var album = new Album("A", "B", 1990) { Id = albumId };
        var song = new Song { Id = songId };

        _albumRepository.Setup(r => r.GetByIdAsync(albumId)).ReturnsAsync(album);
        _songRepository.Setup(r => r.GetByIdAsync(songId)).ReturnsAsync(song);

        await _albumService.AddSongToAlbumAsync(albumId, songId);

        album.Songs.Should().Contain(song);
        song.AlbumId.Should().Be(albumId);
        _albumRepository.Verify(r => r.UpdateAsync(album), Times.Once);
    }

    [Fact]
    public async Task AddSongToAlbumAsync_AlbumMissing_Throws()
    {
        var albumId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        _albumRepository.Setup(r => r.GetByIdAsync(albumId)).ReturnsAsync((Album?)null);

        var act = async () => await _albumService.AddSongToAlbumAsync(albumId, songId);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task IncreaseAlbumPopularityAsync_WhenExists_Updates()
    {
        var albumId = Guid.NewGuid();
        var album = new Album("A", "B", 1990) { Id = albumId, Popularity = 0 };
        _albumRepository.Setup(r => r.GetByIdAsync(albumId)).ReturnsAsync(album);

        await _albumService.IncreaseAlbumPopularityAsync(albumId);

        album.Popularity.Should().Be(1);
        _albumRepository.Verify(r => r.UpdateAsync(album), Times.Once);
    }
}
