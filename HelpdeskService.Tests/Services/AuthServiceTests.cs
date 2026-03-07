using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Settings;
using HelpdeskService.Data.Context;
using HelpdeskService.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpdeskService.Tests.Services;

public class AuthServiceTests
{
    private static HelpdeskDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new HelpdeskDbContext(options);
    }

    private static IOptions<JwtSettings> GetJwtOptions() =>
        Options.Create(new JwtSettings
        {
            SecretKey = "TestSuperSecretKeyForUnitTests_AtLeast32Characters!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        });

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        using var context = CreateInMemoryContext(nameof(RegisterAsync_NewUser_ReturnsAuthResponse));
        var service = new AuthService(context, GetJwtOptions());

        var dto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var result = await service.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("User", result.Role);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsNull()
    {
        using var context = CreateInMemoryContext(nameof(RegisterAsync_DuplicateEmail_ReturnsNull));
        var service = new AuthService(context, GetJwtOptions());

        var dto = new RegisterDto
        {
            Username = "user1",
            Email = "dup@example.com",
            Password = "Password123!"
        };

        await service.RegisterAsync(dto);
        var result = await service.RegisterAsync(dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        using var context = CreateInMemoryContext(nameof(LoginAsync_ValidCredentials_ReturnsAuthResponse));
        var service = new AuthService(context, GetJwtOptions());

        await service.RegisterAsync(new RegisterDto
        {
            Username = "loginuser",
            Email = "login@example.com",
            Password = "Password123!"
        });

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "login@example.com",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.Equal("loginuser", result.Username);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        using var context = CreateInMemoryContext(nameof(LoginAsync_InvalidPassword_ReturnsNull));
        var service = new AuthService(context, GetJwtOptions());

        await service.RegisterAsync(new RegisterDto
        {
            Username = "user2",
            Email = "user2@example.com",
            Password = "CorrectPassword!"
        });

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "user2@example.com",
            Password = "WrongPassword!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsNull()
    {
        using var context = CreateInMemoryContext(nameof(LoginAsync_NonExistentUser_ReturnsNull));
        var service = new AuthService(context, GetJwtOptions());

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "nobody@example.com",
            Password = "Password123!"
        });

        Assert.Null(result);
    }
}
