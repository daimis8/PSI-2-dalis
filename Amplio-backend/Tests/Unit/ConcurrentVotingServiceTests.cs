using FluentAssertions;
using Moq;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services;

namespace Tests.Unit;

public class ConcurrentVotingServiceTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepository;
    private readonly ConcurrentVotingService _votingService;

    public ConcurrentVotingServiceTests()
    {
        _playlistRepository = new Mock<IPlaylistRepository>();
        _votingService = new ConcurrentVotingService(_playlistRepository.Object);
    }

    [Fact]
    public async Task Upvote_IncrementsVotes()
    {
        var song = new Song { Id = Guid.NewGuid(), Title = "T" };
        var playlist = new Playlist("P", true, Guid.NewGuid());
        playlist.AddSong(song);
        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlist.Id)).ReturnsAsync(playlist);

        var playlistSong = playlist.Songs.First();

        await _votingService.UpvoteAsync(playlist.Id, song.Id);
        await _votingService.UpvoteAsync(playlist.Id, song.Id);

        playlistSong.Votes.Should().Be(2);
        (await _votingService.GetVotesAsync(playlist.Id, song.Id)).Should().Be(2);
    }

    [Fact]
    public async Task Upvote_IsThreadSafe()
    {
        var song = new Song { Id = Guid.NewGuid(), Title = "T" };
        var playlist = new Playlist("P", true, Guid.NewGuid());
        playlist.AddSong(song);

        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlist.Id))
            .ReturnsAsync(playlist);

        var tasks = Enumerable.Range(0, 100)
            .Select(_ => _votingService.UpvoteAsync(playlist.Id, song.Id));

        await Task.WhenAll(tasks);

        playlist.Songs.First().Votes.Should().Be(100);
    }

    [Fact]
    public async Task GetVotes_ReturnsCorrectValue()
    {
        var song = new Song { Id = Guid.NewGuid(), Title = "T" };
        var playlist = new Playlist("P", true, Guid.NewGuid());
        playlist.AddSong(song);

        playlist.Songs.First().Votes = 7;

        _playlistRepository.Setup(r => r.GetDetailedByIdAsync(playlist.Id))
            .ReturnsAsync(playlist);

        var result = await _votingService.GetVotesAsync(playlist.Id, song.Id);

        result.Should().Be(7);
    }

}
