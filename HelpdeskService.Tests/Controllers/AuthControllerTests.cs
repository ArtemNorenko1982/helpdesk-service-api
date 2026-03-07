using System.Net;
using System.Net.Http.Json;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Data.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskService.Tests.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        var dbName = $"AuthTestDb_{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<HelpdeskDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<HelpdeskDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        });
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

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
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
