using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Settings;
using HelpdeskService.Data.Context;
using HelpdeskService.Data.Repositories;
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

    private static AuthService CreateService(HelpdeskDbContext context)
    {
        var userRepo = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var tokenService = new TokenService(Options.Create(new JwtSettings
        {
            SecretKey = "TestSuperSecretKeyForUnitTests_AtLeast32Characters!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        }));
        return new AuthService(userRepo, passwordHasher, tokenService);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        using var context = CreateInMemoryContext(nameof(RegisterAsync_NewUser_ReturnsSuccess));
        var service = CreateService(context);

        var result = await service.RegisterAsync(new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("testuser", result.Value.Username);
        Assert.Equal("test@example.com", result.Value.Email);
        Assert.Equal(UserRole.User, result.Value.Role);
        Assert.False(string.IsNullOrEmpty(result.Value.Token));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsConflict()
    {
        using var context = CreateInMemoryContext(nameof(RegisterAsync_DuplicateEmail_ReturnsConflict));
        var service = CreateService(context);

        var dto = new RegisterDto
        {
            Username = "user1",
            Email = "dup@example.com",
            Password = "Password123!"
        };

        await service.RegisterAsync(dto);
        var result = await service.RegisterAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.Conflict, result.Error);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        using var context = CreateInMemoryContext(nameof(LoginAsync_ValidCredentials_ReturnsSuccess));
        var service = CreateService(context);

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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("loginuser", result.Value.Username);
        Assert.False(string.IsNullOrEmpty(result.Value.Token));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsUnauthorized()
    {
        using var context = CreateInMemoryContext(nameof(LoginAsync_InvalidPassword_ReturnsUnauthorized));
        var service = CreateService(context);

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

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.Unauthorized, result.Error);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsUnauthorized()
    {
        using var context = CreateInMemoryContext(nameof(LoginAsync_NonExistentUser_ReturnsUnauthorized));
        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "nobody@example.com",
            Password = "Password123!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.Unauthorized, result.Error);
    }
}
