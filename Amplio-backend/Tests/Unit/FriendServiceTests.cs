using FluentAssertions;
using Moq;
using PSI.Exceptions;
using PSI.Models;
using PSI.Repositories.Interfaces;
using PSI.Services;

namespace Tests.Unit;

public class FriendServiceTests
{
    private readonly Mock<IFriendRepository> _repo = new();
    private readonly FriendService _service;

    private readonly Guid _alice = Guid.NewGuid();
    private readonly Guid _bob = Guid.NewGuid();

    public FriendServiceTests()
    {
        _service = new FriendService(_repo.Object);

        _repo.Setup(r => r.GetUserByIdAsync(_alice))
            .ReturnsAsync(new User { Id = _alice, Username = "alice" });
        _repo.Setup(r => r.GetUserByIdAsync(_bob))
            .ReturnsAsync(new User { Id = _bob, Username = "bob" });
    }

    [Fact]
    public async Task SendRequestAsync_RejectsSelfRequest()
    {
        await Assert.ThrowsAsync<SelfFriendRequestException>(
            () => _service.SendRequestAsync(_alice, _alice));
    }

    [Fact]
    public async Task SendRequestAsync_RejectsWhenBlockExists()
    {
        _repo.Setup(r => r.BlockExistsAsync(_alice, _bob)).ReturnsAsync(true);

        await Assert.ThrowsAsync<BlockedRelationshipException>(
            () => _service.SendRequestAsync(_alice, _bob));
    }

    [Fact]
    public async Task SendRequestAsync_RejectsDuplicatePendingEitherDirection()
    {
        _repo.Setup(r => r.GetPendingAsync(_bob, _alice))
            .ReturnsAsync(new FriendRequest { SenderId = _bob, ReceiverId = _alice });

        await Assert.ThrowsAsync<DuplicateFriendRequestException>(
            () => _service.SendRequestAsync(_alice, _bob));
    }

    [Fact]
    public async Task SendRequestAsync_RejectsWhenAlreadyFriends()
    {
        _repo.Setup(r => r.GetFriendshipAsync(_alice, _bob))
            .ReturnsAsync(new Friendship { UserId = _alice, FriendId = _bob });

        await Assert.ThrowsAsync<AlreadyFriendsException>(
            () => _service.SendRequestAsync(_alice, _bob));
    }

    [Fact]
    public async Task SendRequestAsync_AllowsTwentiethRequestRejectsTwentyFirst()
    {
        _repo.Setup(r => r.CountRequestsSinceAsync(_alice, It.IsAny<DateTime>()))
            .ReturnsAsync(19);

        var dto = await _service.SendRequestAsync(_alice, _bob);
        dto.Status.Should().Be(FriendRequestStatus.Pending);

        _repo.Setup(r => r.CountRequestsSinceAsync(_alice, It.IsAny<DateTime>()))
            .ReturnsAsync(20);

        await Assert.ThrowsAsync<FriendRequestRateLimitException>(
            () => _service.SendRequestAsync(_alice, _bob));
    }

    [Fact]
    public async Task SendRequestAsync_PersistsPendingRequest()
    {
        FriendRequest? captured = null;
        _repo.Setup(r => r.AddRequestAsync(It.IsAny<FriendRequest>()))
            .Callback<FriendRequest>(r => captured = r)
            .Returns(Task.CompletedTask);

        var dto = await _service.SendRequestAsync(_alice, _bob);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(FriendRequestStatus.Pending);
        captured.SenderId.Should().Be(_alice);
        captured.ReceiverId.Should().Be(_bob);
        dto.SenderUsername.Should().Be("alice");
        dto.ReceiverUsername.Should().Be("bob");
    }

    [Fact]
    public async Task AcceptRequestAsync_CreatesFriendshipAndUpdatesStatus()
    {
        var requestId = Guid.NewGuid();
        var request = new FriendRequest
        {
            Id = requestId,
            SenderId = _alice,
            ReceiverId = _bob,
            Status = FriendRequestStatus.Pending
        };
        _repo.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(request);

        Friendship? friendship = null;
        _repo.Setup(r => r.AddFriendshipAsync(It.IsAny<Friendship>()))
            .Callback<Friendship>(f => friendship = f)
            .Returns(Task.CompletedTask);

        var dto = await _service.AcceptRequestAsync(_bob, requestId);

        request.Status.Should().Be(FriendRequestStatus.Accepted);
        friendship.Should().NotBeNull();
        friendship!.UserId.Should().Be(_alice);
        friendship.FriendId.Should().Be(_bob);
        dto.Username.Should().Be("alice");
    }

    [Fact]
    public async Task AcceptRequestAsync_RejectsWhenReceiverDoesNotMatch()
    {
        var requestId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(new FriendRequest
        {
            Id = requestId,
            SenderId = _alice,
            ReceiverId = _bob,
            Status = FriendRequestStatus.Pending
        });

        await Assert.ThrowsAsync<FriendRequestNotFoundException>(
            () => _service.AcceptRequestAsync(_alice, requestId));
    }

    [Fact]
    public async Task DeclineRequestAsync_SetsStatusDeclined()
    {
        var requestId = Guid.NewGuid();
        var request = new FriendRequest
        {
            Id = requestId,
            SenderId = _alice,
            ReceiverId = _bob,
            Status = FriendRequestStatus.Pending
        };
        _repo.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(request);

        await _service.DeclineRequestAsync(_bob, requestId);

        request.Status.Should().Be(FriendRequestStatus.Declined);
        _repo.Verify(r => r.AddFriendshipAsync(It.IsAny<Friendship>()), Times.Never);
    }

    [Fact]
    public async Task CancelRequestAsync_OnlySenderCanCancel()
    {
        var requestId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(new FriendRequest
        {
            Id = requestId,
            SenderId = _alice,
            ReceiverId = _bob,
            Status = FriendRequestStatus.Pending
        });

        await Assert.ThrowsAsync<FriendRequestNotFoundException>(
            () => _service.CancelRequestAsync(_bob, requestId));
    }

    [Fact]
    public async Task RemoveFriendAsync_DeletesFriendship()
    {
        var friendship = new Friendship { UserId = _alice, FriendId = _bob };
        _repo.Setup(r => r.GetFriendshipAsync(_alice, _bob)).ReturnsAsync(friendship);

        await _service.RemoveFriendAsync(_alice, _bob);

        _repo.Verify(r => r.RemoveFriendshipAsync(friendship), Times.Once);
    }

    [Fact]
    public async Task BlockUserAsync_DeletesFriendshipAndCancelsPendingRequests()
    {
        var pendingOut = new FriendRequest { SenderId = _alice, ReceiverId = _bob, Status = FriendRequestStatus.Pending };
        var friendship = new Friendship { UserId = _alice, FriendId = _bob };

        _repo.Setup(r => r.GetPendingAsync(_alice, _bob)).ReturnsAsync(pendingOut);
        _repo.Setup(r => r.GetFriendshipAsync(_alice, _bob)).ReturnsAsync(friendship);

        Block? captured = null;
        _repo.Setup(r => r.AddBlockAsync(It.IsAny<Block>()))
            .Callback<Block>(b => captured = b)
            .Returns(Task.CompletedTask);

        await _service.BlockUserAsync(_alice, _bob);

        pendingOut.Status.Should().Be(FriendRequestStatus.Blocked);
        _repo.Verify(r => r.RemoveFriendshipAsync(friendship), Times.Once);
        captured.Should().NotBeNull();
        captured!.BlockerId.Should().Be(_alice);
        captured.BlockedId.Should().Be(_bob);
    }

    [Fact]
    public async Task BlockUserAsync_IsIdempotent()
    {
        _repo.Setup(r => r.GetBlockAsync(_alice, _bob))
            .ReturnsAsync(new Block { BlockerId = _alice, BlockedId = _bob });

        await _service.BlockUserAsync(_alice, _bob);

        _repo.Verify(r => r.AddBlockAsync(It.IsAny<Block>()), Times.Never);
    }

    [Fact]
    public async Task GetFriendsAsync_ReturnsOtherSideOfEachFriendship()
    {
        var third = Guid.NewGuid();
        _repo.Setup(r => r.GetUserByIdAsync(third))
            .ReturnsAsync(new User { Id = third, Username = "carol" });

        _repo.Setup(r => r.GetFriendshipsAsync(_alice)).ReturnsAsync(new List<Friendship>
        {
            new()
            {
                UserId = _alice, FriendId = _bob,
                User = new User { Id = _alice, Username = "alice" },
                Friend = new User { Id = _bob, Username = "bob" }
            },
            new()
            {
                UserId = third, FriendId = _alice,
                User = new User { Id = third, Username = "carol" },
                Friend = new User { Id = _alice, Username = "alice" }
            }
        });

        var friends = await _service.GetFriendsAsync(_alice);

        friends.Select(f => f.Username).Should().BeEquivalentTo(new[] { "bob", "carol" });
    }
}
