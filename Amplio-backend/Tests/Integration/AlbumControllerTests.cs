using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PSI;
using PSI.Data;
using PSI.DTOs;
using PSI.Models;
using System.Linq;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration;

public class AlbumControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AlbumControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllAlbums_ReturnsOk()
    {
        var response = await _client.GetAsync("/albums");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAlbum_Then_GetById_ReturnsCreated_Then_Ok()
    {
        var createRequest = new AlbumDto
        {
            Name = "Test Album",
            Artist = "Test Artist",
            ReleaseYear = 2000
        };

        var created = await _client.PostAsJsonAsync("/albums", createRequest);

        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = created.Headers.Location;
        location.Should().NotBeNull();

        var get = await _client.GetAsync(location!);
        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAlbum_InvalidRequest_ReturnsBadRequest()
    {
        var bad = new AlbumDto { Name = "", Artist = "Artist", ReleaseYear = 1999 };
        var response = await _client.PostAsJsonAsync("/albums", bad);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAlbum_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/albums/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSongToAlbum_Valid_ReturnsOk_AndSongLinked()
    {
        Guid albumId;
        Guid songId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var album = new Album("Integration Album", "Artist", 1998);
            db.Albums.Add(album);
            var song = new Song { Title = "SongX", Artist = "Artist" };
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            albumId = album.Id;
            songId = song.Id;
        }

        var response = await _client.PostAsync($"/albums/{albumId}/songs/{songId}", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var album = db.Albums.Include(a => a.Songs).First(a => a.Id == albumId);
            album.Songs.Should().Contain(s => s.Id == songId);
        }
    }

    [Fact]
    public async Task AddSongToAlbum_MissingAlbum_ReturnsNotFound()
    {
        Guid songId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var song = new Song { Title = "Lonely", Artist = "A" };
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            songId = song.Id;
        }

        var response = await _client.PostAsync($"/albums/{Guid.NewGuid()}/songs/{songId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
