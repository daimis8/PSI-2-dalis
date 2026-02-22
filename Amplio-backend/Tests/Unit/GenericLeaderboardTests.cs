using FluentAssertions;
using PSI.Models;

namespace Tests.Unit;

public class GenericLeaderboardTests
{
    [Fact]
    public void GenericLeaderboard_Constructor_InitializesEmpty()
    {
        var leaderboard = new GenericLeaderboard<Album>();

        leaderboard.LeaderboardItems.Should().NotBeNull();
        leaderboard.LeaderboardItems.Should().BeEmpty();
    }

    [Fact]
    public void AddSongCollection_AddsSingleItem()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var album = new Album("Test", "Artist", 2000);

        leaderboard.AddSongCollection(album);

        leaderboard.LeaderboardItems.Should().HaveCount(1);
        leaderboard.LeaderboardItems.Should().Contain(album);
    }

    [Fact]
    public void AddSongCollection_AddsUnique()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var a1 = new Album("A","B",2000);
        leaderboard.AddSongCollection(a1);
        leaderboard.AddSongCollection(a1);
        leaderboard.LeaderboardItems.Should().HaveCount(1);
    }

    [Fact]
    public void AddSongCollection_AddsMultipleUniqueItems()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var a1 = new Album("A", "Artist", 2000);
        var a2 = new Album("B", "Artist", 2001);
        var a3 = new Album("C", "Artist", 2002);

        leaderboard.AddSongCollection(a1);
        leaderboard.AddSongCollection(a2);
        leaderboard.AddSongCollection(a3);

        leaderboard.LeaderboardItems.Should().HaveCount(3);
    }

    [Fact]
    public void GetSortedByPopularity_SortsCorrectly()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var a1 = new Album("A","B",2000) { Popularity = 5 };
        var a2 = new Album("B","B",2000) { Popularity = 10 };
        leaderboard.AddSongCollection(a1);
        leaderboard.AddSongCollection(a2);
        var sorted = leaderboard.GetSortedByPopularity();
        sorted.First().Popularity.Should().Be(10);
    }

    [Fact]
    public void SortByPopularity_SortsInPlace()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var albums = new[]
        {
            new Album("A", "Artist", 2000) { Popularity = 5 },
            new Album("B", "Artist", 2001) { Popularity = 10 },
            new Album("C", "Artist", 2002) { Popularity = 3 }
        };

        foreach (var album in albums)
        {
            leaderboard.AddSongCollection(album);
        }

        leaderboard.SortByPopularity();

        leaderboard.LeaderboardItems[0].Popularity.Should().Be(10);
        leaderboard.LeaderboardItems[1].Popularity.Should().Be(5);
        leaderboard.LeaderboardItems[2].Popularity.Should().Be(3);
    }

    [Fact]
    public void GetSortedByPopularity_EmptyLeaderboard_ReturnsEmpty()
    {
        var leaderboard = new GenericLeaderboard<Album>();

        var sorted = leaderboard.GetSortedByPopularity();

        sorted.Should().BeEmpty();
    }

    [Fact]
    public void GetSortedByPopularity_WithTies_SortsByName()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var a1 = new Album("C Album", "Artist", 2000) { Popularity = 5 };
        var a2 = new Album("A Album", "Artist", 2001) { Popularity = 5 };
        var a3 = new Album("B Album", "Artist", 2002) { Popularity = 5 };

        leaderboard.AddSongCollection(a1);
        leaderboard.AddSongCollection(a2);
        leaderboard.AddSongCollection(a3);

        var sorted = leaderboard.GetSortedByPopularity();

        sorted.Should().HaveCount(3);
        sorted[0].Name.Should().Be("A Album");
        sorted[1].Name.Should().Be("B Album");
        sorted[2].Name.Should().Be("C Album");
    }

    [Fact]
    public void RemoveSongList_RemovesItem()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var album = new Album("Test", "Artist", 2000);
        leaderboard.AddSongCollection(album);

        leaderboard.RemoveSongList(album.Id);

        leaderboard.LeaderboardItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSongList_NonExistentId_DoesNotThrow()
    {
        var leaderboard = new GenericLeaderboard<Album>();

        var act = () => leaderboard.RemoveSongList(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void GetPropertyOfMostPopular_ReturnsCorrectProperty()
    {
        var leaderboard = new GenericLeaderboard<Album>();
        var a1 = new Album("Less Popular", "Artist", 2000) { Popularity = 5 };
        var a2 = new Album("Most Popular", "Artist", 2001) { Popularity = 10 };

        leaderboard.AddSongCollection(a1);
        leaderboard.AddSongCollection(a2);

        var name = leaderboard.GetPropertyOfMostPopular(a => a.Name);

        name.Should().Be("Most Popular");
    }

    [Fact]
    public void GetPropertyOfMostPopular_EmptyLeaderboard_Throws()
    {
        var leaderboard = new GenericLeaderboard<Album>();

        var act = () => leaderboard.GetPropertyOfMostPopular(a => a.Name);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenericLeaderboard_WorksWithPlaylists()
    {
        var leaderboard = new GenericLeaderboard<Playlist>();
        var p1 = new Playlist("P1", true, Guid.NewGuid()) { Popularity = 3 };
        var p2 = new Playlist("P2", true, Guid.NewGuid()) { Popularity = 8 };

        leaderboard.AddSongCollection(p1);
        leaderboard.AddSongCollection(p2);

        var sorted = leaderboard.GetSortedByPopularity();

        sorted.First().Popularity.Should().Be(8);
        sorted.Last().Popularity.Should().Be(3);
    }
}
