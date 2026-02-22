using FluentAssertions;
using PSI.Models;

namespace Tests.Unit;

public class AlbumModelTests
{
    [Fact]
    public void Album_Constructor_InitializesProperties()
    {
        var album = new Album("Test Album", "Test Artist", 2000);

        album.Name.Should().Be("Test Album");
        album.Artist.Should().Be("Test Artist");
        album.ReleaseYear.Should().Be(2000);
        album.Popularity.Should().Be(0);
        album.Songs.Should().NotBeNull();
        album.Songs.Should().BeEmpty();
    }

    [Fact]
    public void AddSong_AddsSongToAlbum()
    {
        var album = new Album("A", "B", 2000);
        var song = new Song { Id = Guid.NewGuid(), Title = "X", Artist = "B" };

        album.AddSong(song);

        album.Songs.Should().HaveCount(1);
        album.Songs.Should().Contain(song);
    }

    [Fact]
    public void AddSong_DoesNotDuplicate()
    {
        var album = new Album("A","B",2000);
        var song = new Song { Id = Guid.NewGuid(), Title = "X" };

        album.AddSong(song);
        album.AddSong(song);

        album.Songs.Should().HaveCount(1);
        album.GetAllSongs().Should().ContainSingle(s => s.Id == song.Id);
    }


    [Fact]
    public void GetAllSongs_ReturnsAllSongs()
    {
        var album = new Album("A", "B", 2000);
        var song1 = new Song { Title = "Song 1" };
        var song2 = new Song { Title = "Song 2" };

        album.AddSong(song1);
        album.AddSong(song2);

        var allSongs = album.GetAllSongs();
        allSongs.Should().HaveCount(2);
        allSongs.Should().Contain(song1);
        allSongs.Should().Contain(song2);
    }

    [Fact]
    public void IncreasePopularity_Increments()
    {
        var album = new Album("A","B",2000);
        album.Popularity.Should().Be(0);
        album.IncreasePopularity();
        album.Popularity.Should().Be(1);
    }

    [Fact]
    public void IncreasePopularity_IncrementsTwice()
    {
        var album = new Album("A","B",2000);
        album.IncreasePopularity();
        album.IncreasePopularity();
        album.Popularity.Should().Be(2);
    }

    [Fact]
    public void Album_CanEnumerateSongs()
    {
        var album = new Album("A", "B", 2000);
        var song1 = new Song { Title = "Song 1" };
        var song2 = new Song { Title = "Song 2" };
        
        album.AddSong(song1);
        album.AddSong(song2);

        var count = 0;
        foreach (var song in album.Songs)
        {
            count++;
        }
        count.Should().Be(2);
    }

    [Fact]
    public void Album_DefaultConstructor_InitializesWithDefaults()
    {
        var album = new Album();

        album.Songs.Should().NotBeNull();
        album.Popularity.Should().Be(0);
    }

    [Fact]
    public void Album_WithEmptyName_StillWorks()
    {
        var album = new Album("", "Artist", 2000);
        album.Name.Should().Be("");
    }

    [Fact]
    public void Album_SetProperties_StoresValues()
    {
        var album = new Album
        {
            Name = "Set Name",
            Artist = "Set Artist",
            ReleaseYear = 1999
        };

        album.Name.Should().Be("Set Name");
        album.Artist.Should().Be("Set Artist");
        album.ReleaseYear.Should().Be(1999);
    }
}
