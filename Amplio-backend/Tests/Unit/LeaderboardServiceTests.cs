using FluentAssertions;
using Moq;
using PSI.DTOs;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services;

namespace Tests.Unit;

public class LeaderboardServiceTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepository;
    private readonly Mock<IAlbumRepository> _albumRepository;

    private readonly LeaderboardService _leaderboardService;

    public LeaderboardServiceTests()
    {
        _playlistRepository = new Mock<IPlaylistRepository>();
        _albumRepository = new Mock<IAlbumRepository>();

        _leaderboardService = new LeaderboardService(
            _playlistRepository.Object,
            _albumRepository.Object
        );
    }

    [Fact]
    public async Task GetPlaylistLeaderboardAsync_ReturnsSorted()
    {
        var p1 = new Playlist("P1", true, Guid.NewGuid()) { Popularity = 1 };
        var p2 = new Playlist("P2", true, Guid.NewGuid()) { Popularity = 5 };
        _playlistRepository.Setup(r => r.GetPublicPlaylistsAsync()).ReturnsAsync(new List<Playlist> { p1, p2 });
        
        var result = await _leaderboardService.GetPlaylistLeaderboardAsync();
        result.LeaderboardItems.Should().HaveCount(2);
        result.LeaderboardItems.First().Popularity.Should().Be(5);
    }

    [Fact]
    public async Task GetAlbumLeaderboardAsync_ReturnsSorted()
    {
        var a1 = new Album("A","B",2000) { Popularity = 2 };
        var a2 = new Album("B","B",2000) { Popularity = 10 };
        _albumRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Album> { a1, a2 });
        
        var result = await _leaderboardService.GetAlbumLeaderboardAsync();
        result.LeaderboardItems.Should().HaveCount(2);
        result.LeaderboardItems.First().Popularity.Should().Be(10);
    }
}
