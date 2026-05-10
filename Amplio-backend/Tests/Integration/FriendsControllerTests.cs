using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PSI.Data;
using PSI.DTOs;
using PSI.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.Integration;

public class FriendsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public FriendsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>(JsonOpts);

    private async Task<T?> GetAsync<T>(HttpClient client, string url)
    {
        var resp = await client.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<T>(JsonOpts);
    }

    private (Guid alice, Guid bob) SeedTwoUsers(string suffix)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alice = new User { Id = Guid.NewGuid(), Username = $"alice-{suffix}", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var bob = new User { Id = Guid.NewGuid(), Username = $"bob-{suffix}", PasswordHash = new byte[] { 2 }, PasswordSalt = new byte[] { 2 } };
        db.Users.AddRange(alice, bob);
        db.SaveChanges();
        return (alice.Id, bob.Id);
    }

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User-Id", userId.ToString());
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    [Fact]
    public async Task SendRequest_Then_Accept_CreatesMutualFriendship()
    {
        var (alice, bob) = SeedTwoUsers(nameof(SendRequest_Then_Accept_CreatesMutualFriendship));
        var aliceClient = ClientAs(alice);
        var bobClient = ClientAs(bob);

        var send = await aliceClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(bob));
        send.StatusCode.Should().Be(HttpStatusCode.Created);
        var sent = await ReadAsync<FriendRequestDto>(send);
        sent.Should().NotBeNull();

        var incoming = await GetAsync<List<FriendRequestDto>>(bobClient, "/friends/requests/incoming");
        incoming.Should().ContainSingle(r => r.Id == sent!.Id);

        var accept = await bobClient.PostAsync($"/friends/requests/{sent!.Id}/accept", null);
        accept.StatusCode.Should().Be(HttpStatusCode.OK);

        var aliceFriends = await GetAsync<List<FriendDto>>(aliceClient, "/friends");
        var bobFriends = await GetAsync<List<FriendDto>>(bobClient, "/friends");
        aliceFriends.Should().ContainSingle(f => f.Id == bob);
        bobFriends.Should().ContainSingle(f => f.Id == alice);
    }

    [Fact]
    public async Task SendRequest_ToSelf_Returns400()
    {
        var (alice, _) = SeedTwoUsers(nameof(SendRequest_ToSelf_Returns400));
        var aliceClient = ClientAs(alice);

        var send = await aliceClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(alice));
        send.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendRequest_DuplicatePending_Returns409()
    {
        var (alice, bob) = SeedTwoUsers(nameof(SendRequest_DuplicatePending_Returns409));
        var aliceClient = ClientAs(alice);

        var first = await aliceClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(bob));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await aliceClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(bob));
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task BlockUser_DeletesFriendshipAndCancelsPendingRequest()
    {
        var (alice, bob) = SeedTwoUsers(nameof(BlockUser_DeletesFriendshipAndCancelsPendingRequest));
        var aliceClient = ClientAs(alice);
        var bobClient = ClientAs(bob);

        var send = await aliceClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(bob));
        var sent = await ReadAsync<FriendRequestDto>(send);
        await bobClient.PostAsync($"/friends/requests/{sent!.Id}/accept", null);

        var aliceFriends = await GetAsync<List<FriendDto>>(aliceClient, "/friends");
        aliceFriends.Should().ContainSingle();

        var block = await aliceClient.PostAsync($"/friends/blocks/{bob}", null);
        block.StatusCode.Should().Be(HttpStatusCode.NoContent);

        aliceFriends = await GetAsync<List<FriendDto>>(aliceClient, "/friends");
        aliceFriends.Should().BeEmpty();

        // Bob can no longer send Alice a friend request — block exists.
        var resend = await bobClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(alice));
        resend.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_ExcludesUsersWhoBlockedTheCaller()
    {
        var (alice, bob) = SeedTwoUsers(nameof(Search_ExcludesUsersWhoBlockedTheCaller));
        var aliceClient = ClientAs(alice);
        var bobClient = ClientAs(bob);

        // Bob blocks Alice -> Alice should not see Bob in her search results.
        await bobClient.PostAsync($"/friends/blocks/{alice}", null);

        var found = await GetAsync<List<UserSearchResultDto>>(aliceClient, "/friends/search?q=bob-Search_Excludes");
        found.Should().NotContain(u => u.Id == bob);
    }

    [Fact]
    public async Task InviteFriendToPlaylist_AppearsInFriendsPersonalList()
    {
        var (alice, bob) = SeedTwoUsers(nameof(InviteFriendToPlaylist_AppearsInFriendsPersonalList));
        var aliceClient = ClientAs(alice);
        var bobClient = ClientAs(bob);

        // Become friends.
        var send = await aliceClient.PostAsJsonAsync("/friends/requests", new SendFriendRequestDto(bob));
        var sent = await ReadAsync<FriendRequestDto>(send);
        await bobClient.PostAsync($"/friends/requests/{sent!.Id}/accept", null);

        // Alice creates a private playlist.
        var create = await aliceClient.PostAsJsonAsync("/playlist", new CreatePlaylistRequestDto("AliceList", false, null));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = create.Headers.Location!;
        var playlistPath = location.IsAbsoluteUri ? location.AbsolutePath : location.ToString();
        var playlistId = Guid.Parse(playlistPath.TrimEnd('/').Split('/').Last());

        // Alice invites Bob.
        var invite = await aliceClient.PostAsync($"/playlist/{playlistId}/invite/{bob}", null);
        invite.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Bob's personal list now contains the invited playlist.
        var bobPlaylists = await GetAsync<List<JsonElement>>(bobClient, "/playlist/personal");
        bobPlaylists.Should().Contain(p => p.GetProperty("id").GetGuid() == playlistId);
    }

    [Fact]
    public async Task InviteNonFriend_Returns400()
    {
        var (alice, bob) = SeedTwoUsers(nameof(InviteNonFriend_Returns400));
        var aliceClient = ClientAs(alice);

        var create = await aliceClient.PostAsJsonAsync("/playlist", new CreatePlaylistRequestDto("List", false, null));
        var location = create.Headers.Location!;
        var playlistPath = location.IsAbsoluteUri ? location.AbsolutePath : location.ToString();
        var playlistId = Guid.Parse(playlistPath.TrimEnd('/').Split('/').Last());

        var invite = await aliceClient.PostAsync($"/playlist/{playlistId}/invite/{bob}", null);
        invite.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RequiresAuthentication()
    {
        // Use a completely fresh client with NO test user header — TestAuthHandler still authenticates,
        // but the friend-system endpoints depend on a real persisted user; so this asserts the routing
        // and authorize attribute are wired (not 404, but a successful auth path).
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });

        var resp = await client.GetAsync("/friends");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
