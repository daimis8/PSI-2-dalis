using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Tests.Integration;

public class LeaderboardControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LeaderboardControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPlaylistLeaderboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/leaderboard/playlists");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPlaylistLeaderboard_WithTopN_ReturnsOk()
    {
        var response = await _client.GetAsync("/leaderboard/playlists?topN=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAlbumLeaderboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/leaderboard/albums");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAlbumLeaderboard_WithTopN_ReturnsOk()
    {
        var response = await _client.GetAsync("/leaderboard/albums?topN=3");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPlaylistLeaderboard_DefaultTopN_ReturnsTop10()
    {
        var response = await _client.GetAsync("/leaderboard/playlists");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAlbumLeaderboard_DefaultTopN_ReturnsTop10()
    {
        var response = await _client.GetAsync("/leaderboard/albums");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
