using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PSI.Controllers;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidCredentials_ReturnsOkWithToken()
    {
        var request = new LoginRequest
        {
            Username = $"user{Guid.NewGuid().ToString()[..8]}",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_EmptyUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "12345"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShortUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "ab",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_LongUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = new string('a', 21),
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        var username = $"dupuser{Guid.NewGuid().ToString()[..8]}";
        var request = new LoginRequest
        {
            Username = username,
            Password = "password123"
        };

        var first = await _client.PostAsJsonAsync("/auth/register", request);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/auth/register", request);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var username = $"loginuser{Guid.NewGuid().ToString()[..8]}";
        var request = new LoginRequest
        {
            Username = username,
            Password = "password123"
        };

        await _client.PostAsJsonAsync("/auth/register", request);

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", request);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var username = $"pwdtest{Guid.NewGuid().ToString()[..8]}";
        var registerRequest = new LoginRequest
        {
            Username = username,
            Password = "correctpassword"
        };

        await _client.PostAsJsonAsync("/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmptyCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync("/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
