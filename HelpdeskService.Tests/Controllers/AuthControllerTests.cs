using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Data.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskService.Tests.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        var dbName = $"AuthTestDb_{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceDbContext(services, dbName);
            });
        });
    }

    internal static void ReplaceDbContext(IServiceCollection services, string dbName)
    {
        var toRemove = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<HelpdeskDbContext>)
                     || d.ServiceType == typeof(HelpdeskDbContext))
            .ToList();
        foreach (var descriptor in toRemove)
            services.Remove(descriptor);

        services.AddDbContext<HelpdeskDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
    }

    [Fact]
    public async Task Register_ValidData_Returns201()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var dto = new RegisterDto
        {
            Username = "dupuser",
            Email = "dup@test.com",
            Password = "Password123!"
        };

        await client.PostAsJsonAsync("/api/v1/auth/register", dto);
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenResponse()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterDto
        {
            Username = "logintest",
            Email = "logintest@example.com",
            Password = "Password123!"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto
        {
            Email = "logintest@example.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponseDto>(json, JsonOptions);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal(UserRole.User, result.Role);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto
        {
            Email = "nobody@example.com",
            Password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
