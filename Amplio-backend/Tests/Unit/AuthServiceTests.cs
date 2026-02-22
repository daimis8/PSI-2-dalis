using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PSI.Data;
using PSI.Exceptions;
using PSI.Services;

namespace Tests.Unit;

public class AuthServiceTests
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuthTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);

        var configDict = new Dictionary<string, string>
        {
            { "Jwt:Key", "SuperSecretKeyThatIsAtLeast32CharactersLong!" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        _authService = new AuthService(_context, _configuration);
    }

    [Fact]
    public async Task Register_ValidCredentials_ReturnsToken()
    {
        var token = await _authService.Register("testuser", "password123");
        
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_EmptyUsername_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.Register("", "password123"));
    }

    [Fact]
    public async Task Register_EmptyPassword_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.Register("testuser", ""));
    }

    [Fact]
    public async Task Register_ShortUsername_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.Register("ab", "password123"));
    }

    [Fact]
    public async Task Register_LongUsername_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.Register(new string('a', 21), "password123"));
    }

    [Fact]
    public async Task Register_ShortPassword_ThrowsInvalidPasswordException()
    {
        await Assert.ThrowsAsync<InvalidPasswordException>(() =>
            _authService.Register("testuser", "12345"));
    }

    [Fact]
    public async Task Register_DuplicateUsername_ThrowsUsernameAlreadyExistsException()
    {
        await _authService.Register("duplicate", "password123");
        
        await Assert.ThrowsAsync<UsernameAlreadyExistsException>(() =>
            _authService.Register("duplicate", "password456"));
    }

    [Fact]
    public async Task Register_CreatesUserInDatabase()
    {
        await _authService.Register("dbuser", "password123");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "dbuser");
        user.Should().NotBeNull();
        user!.Username.Should().Be("dbuser");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await _authService.Register("loginuser", "password123");

        var token = await _authService.Login("loginuser", "password123");
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidUsername_ThrowsInvalidCredentialsException()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.Login("nonexistent", "password123"));
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsInvalidCredentialsException()
    {
        await _authService.Register("user", "correctpassword");

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.Login("user", "wrongpassword"));
    }

    [Fact]
    public async Task Login_EmptyUsername_ThrowsInvalidCredentialsException()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.Login("", "password123"));
    }

    [Fact]
    public async Task Login_EmptyPassword_ThrowsInvalidCredentialsException()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.Login("user", ""));
    }

    [Fact]
    public async Task Register_ThenLogin_BothReturnTokens()
    {
        var registerToken = await _authService.Register("fulltest", "password123");
        var loginToken = await _authService.Login("fulltest", "password123");

        registerToken.Should().NotBeNullOrEmpty();
        loginToken.Should().NotBeNullOrEmpty();
    }
}
