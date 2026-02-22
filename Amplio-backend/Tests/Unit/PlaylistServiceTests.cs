using FluentAssertions;
using Moq;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services;
using PSI.Services.Interfaces;

namespace Tests.Unit;

public class PlaylistServiceTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepository;
    private readonly Mock<ISongRepository> _songRepository;
    private readonly Mock<IAlbumRepository> _albumRepository;
    private readonly Mock<IConcurrentVotingService> _votingService;

    private readonly PlaylistService _playlistService;

    public PlaylistServiceTests()
    {
        _playlistRepository = new Mock<IPlaylistRepository>();
        _songRepository = new Mock<ISongRepository>();
        _albumRepository = new Mock<IAlbumRepository>();
        _votingService = new Mock<IConcurrentVotingService>();

        _playlistService = new PlaylistService(
            _playlistRepository.Object,
            _songRepository.Object,
            _albumRepository.Object,
            _votingService.Object
        );
    }

    [Fact]
    public async Task CreatePlaylistAsync_Valid_Creates()
    {
        var playlist = await _playlistService.CreatePlaylistAsync("My", true, Guid.NewGuid(), Guid.NewGuid());
        playlist.Name.Should().Be("My");
        _playlistRepository.Verify(r => r.AddAsync(It.IsAny<Playlist>()), Times.Once);
    }

    [Fact]
    public async Task CreatePlaylistAsync_InvalidName_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _playlistService.CreatePlaylistAsync("", true, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task AddSongToPlaylistAsync_Adds_Song_And_Updates()
    {
        var playlistId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        var playlist = new Playlist("P", true, Guid.NewGuid()) { Id = playlistId };
        var song = new Song { Id = songId };
        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlistId)).ReturnsAsync(playlist);
        _songRepository.Setup(r => r.GetByIdAsync(songId)).ReturnsAsync(song);

        var songs = await _playlistService.AddSongToPlaylistAsync(playlistId, songId);
        songs.Should().HaveCount(1);
        _playlistRepository.Verify(r => r.UpdateAsync(playlist), Times.Once);
    }

    [Fact]
    public async Task RemoveSongFromPlaylistAsync_Removes()
    {
        var playlistId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        var playlist = new Playlist("P", true, Guid.NewGuid()) { Id = playlistId };
        playlist.AddSong(new Song { Id = songId });
        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlistId)).ReturnsAsync(playlist);

        await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId);
        playlist.Songs.Should().BeEmpty();
    }

    [Fact]
    public async Task UpvoteSongInPlaylistAsync_CallsVotingService()
    {
        var playlistId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        var playlist = new Playlist("P", true, Guid.NewGuid()) { Id = playlistId };
        var song = new Song { Id = songId };
        playlist.AddSong(song);
        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlistId)).ReturnsAsync(playlist);

        var result = await _playlistService.UpvoteSongInPlaylistAsync(playlistId, songId);
        result.Should().HaveCount(1);
        _votingService.Verify(v => v.UpvoteAsync(playlistId, songId), Times.Once);
    }

    [Fact]
    public async Task SetCurrentSongAsync_SetsAndRemovesFromList()
    {
        var playlistId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        var playlist = new Playlist("P", true, Guid.NewGuid()) { Id = playlistId };
        playlist.AddSong(new Song { Id = songId });
        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlistId)).ReturnsAsync(playlist);
        
        var current = await _playlistService.SetCurrentSongAsync(playlistId);
        current.Id.Should().Be(songId);
        playlist.Songs.Should().BeEmpty();
    }
}
