using FluentAssertions;
using PSI.Models;

namespace Tests.Unit;

public class PlaylistModelTests
{
    [Fact]
    public void AddSong_NoDuplicates()
    {
        var playlist = new Playlist("P", true, Guid.NewGuid());
        var song = new Song { Id = Guid.NewGuid(), Title = "T" };
        playlist.AddSong(song);
        playlist.AddSong(song);
        playlist.Songs.Should().HaveCount(1);
    }

    [Fact]
    public void IncreasePopularity_IncrementsVisitCount()
    {
        var playlist = new Playlist("P", true, Guid.NewGuid());
        playlist.VisitCount.Should().Be(0);
        playlist.IncreasePopularity();
        playlist.VisitCount.Should().Be(1);
        playlist.Popularity.Should().Be(1);
    }
}
