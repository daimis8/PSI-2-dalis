using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PSI.DTOs;
using PSI.Data;
using PSI.Models;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class PlaylistControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PlaylistControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });
    }

    private static Guid ExtractIdFromLocation(Uri? location)
    {
        location.Should().NotBeNull();
        var path = location!.IsAbsoluteUri ? location.AbsolutePath : location.ToString();
        var idPart = path.TrimEnd('/').Split('/').Last();
        return Guid.Parse(idPart);
    }

    [Fact]
    public async Task CreatePlaylist_ValidRequest_ReturnsCreated()
    {
        var createRequest = new CreatePlaylistRequestDto("MyList", true, null);
        var response = await _client.PostAsJsonAsync("/playlist", createRequest);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = ExtractIdFromLocation(response.Headers.Location);
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePlaylist_CurrentSongNotSet_ReturnsNoContent()
    {
        var createRequest = new CreatePlaylistRequestDto("MyList", true, null);
        var created = await _client.PostAsJsonAsync("/playlist", createRequest);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = ExtractIdFromLocation(created.Headers.Location);

        var current = await _client.GetAsync($"/playlist/{id}/current");
        current.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task VisitPlaylist_IncreasesPopularity()
    {
        var createRequest = new CreatePlaylistRequestDto("VisitMe", true, null);
        var created = await _client.PostAsJsonAsync("/playlist", createRequest);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = ExtractIdFromLocation(created.Headers.Location);

        var visit = await _client.PostAsync($"/playlist/{id}/visit", null);
        visit.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSongsInPlaylist_EmptyPlaylist_ReturnsEmptyList()
    {
        var createRequest = new CreatePlaylistRequestDto("Empty", true, null);
        var created = await _client.PostAsJsonAsync("/playlist", createRequest);
        var id = ExtractIdFromLocation(created.Headers.Location);

        var response = await _client.GetAsync($"/playlist/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var songs = await response.Content.ReadFromJsonAsync<List<object>>();
        songs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSongsInPlaylist_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/playlist/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlaylistName_ValidPlaylist_ReturnsName()
    {
        var createRequest = new CreatePlaylistRequestDto("NameTest", true, null);
        var created = await _client.PostAsJsonAsync("/playlist", createRequest);
        var id = ExtractIdFromLocation(created.Headers.Location);

        var response = await _client.GetAsync($"/playlist/{id}/name");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var name = await response.Content.ReadAsStringAsync();
        name.Should().Contain("NameTest");
    }

    [Fact]
    public async Task AddSongToPlaylist_ValidSong_ReturnsOk()
    {
        Guid playlistId;
        Guid songId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var playlist = new Playlist("TestPlaylist", true, Guid.NewGuid());
            db.Playlists.Add(playlist);
            var song = new Song { Title = "TestSong", Artist = "TestArtist" };
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            playlistId = playlist.Id;
            songId = song.Id;
        }

        var response = await _client.PostAsync($"/playlist/{playlistId}/add/{songId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddSongToPlaylist_NonExistentPlaylist_ReturnsNotFound()
    {
        Guid songId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var song = new Song { Title = "Song", Artist = "Artist" };
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            songId = song.Id;
        }

        var response = await _client.PostAsync($"/playlist/{Guid.NewGuid()}/add/{songId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveSongFromPlaylist_ValidSong_ReturnsOk()
    {
        Guid playlistId;
        Guid songId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var playlist = new Playlist("RemoveTest", true, Guid.NewGuid());
            var song = new Song { Title = "Remove", Artist = "Me" };
            db.Songs.Add(song);
            playlist.AddSong(song);
            db.Playlists.Add(playlist);
            await db.SaveChangesAsync();
            playlistId = playlist.Id;
            songId = song.Id;
        }

        var response = await _client.DeleteAsync($"/playlist/{playlistId}/delete/{songId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpvoteSongInPlaylist_ValidSong_ReturnsOk()
    {
        Guid playlistId;
        Guid songId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var playlist = new Playlist("VoteTest", true, Guid.NewGuid());
            var song = new Song { Title = "Vote", Artist = "Me" };
            db.Songs.Add(song);
            playlist.AddSong(song);
            db.Playlists.Add(playlist);
            await db.SaveChangesAsync();
            playlistId = playlist.Id;
            songId = song.Id;
        }

        var response = await _client.PostAsync($"/playlist/{playlistId}/vote/{songId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetCurrentSong_EmptyPlaylist_ReturnsBadRequest()
    {
        var createRequest = new CreatePlaylistRequestDto("Empty", true, null);
        var created = await _client.PostAsJsonAsync("/playlist", createRequest);
        var id = ExtractIdFromLocation(created.Headers.Location);

        var response = await _client.PostAsync($"/playlist/{id}/play", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetCurrentSong_WithSongs_ReturnsOk()
    {
        Guid playlistId;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var playlist = new Playlist("PlayTest", true, Guid.NewGuid());
            var song = new Song { Title = "Play", Artist = "Me" };
            db.Songs.Add(song);
            playlist.AddSong(song);
            db.Playlists.Add(playlist);
            await db.SaveChangesAsync();
            playlistId = playlist.Id;
        }

        var response = await _client.PostAsync($"/playlist/{playlistId}/play", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ClearCurrentSong_ValidPlaylist_ReturnsOk()
    {
        var createRequest = new CreatePlaylistRequestDto("ClearTest", true, null);
        var created = await _client.PostAsJsonAsync("/playlist", createRequest);
        var id = ExtractIdFromLocation(created.Headers.Location);

        var response = await _client.DeleteAsync($"/playlist/{id}/current");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyPlaylists_ReturnsOk()
    {
        var response = await _client.GetAsync("/playlist/personal");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
