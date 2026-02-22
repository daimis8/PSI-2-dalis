using FluentAssertions;
using System.Net;
using PSI.Data;
using PSI.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;


namespace Tests.Integration;

public class SongsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SongsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllSongs_EmptyInitially_Ok()
    {
        var response = await _client.GetAsync("/songs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllSongs_ReturnsList()
    {
        var response = await _client.GetAsync("/songs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var songs = await response.Content.ReadFromJsonAsync<List<object>>();
        songs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSongById_NotFound()
    {
        var response = await _client.GetAsync($"/songs/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSongById_ExistingSong_ReturnsOk()
    {
        Guid songId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var song = new Song { Title = "Test Song", Artist = "Test Artist" };
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            songId = song.Id;
        }

        var response = await _client.GetAsync($"/songs/{songId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSongLink_ExistingSong_ReturnsOk()
    {
        Guid songId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var song = new Song 
            { 
                Title = "Link Test", 
                Artist = "Artist",
                Link = new SongLink("http://example.com")
            };
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            songId = song.Id;
        }

        var response = await _client.GetAsync($"/songs/play/{songId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSongLink_NonExistentSong_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/songs/play/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadSongs_FileMissing_ReturnsNotFound()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "DummyData");
        var filePath = Path.Combine(dir, "songs.json");
        if (File.Exists(filePath)) File.Delete(filePath);

        var response = await _client.PostAsync("/songs/upload", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
