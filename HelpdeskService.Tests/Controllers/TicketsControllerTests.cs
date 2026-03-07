using System.Net;
using System.Net.Http.Headers;
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

public class TicketsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public TicketsControllerTests(WebApplicationFactory<Program> factory)
    {
        var dbName = $"TicketsTestDb_{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                AuthControllerTests.ReplaceDbContext(services, dbName);
            });
        });
    }

    private async Task<string> GetTokenAsync(HttpClient client, string suffix = "")
    {
        var email = $"user{suffix}@tickets.test";
        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterDto
        {
            Username = $"ticketuser{suffix}",
            Email = email,
            Password = "Password123!"
        });

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto
        {
            Email = email,
            Password = "Password123!"
        });

        var json = await loginResponse.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponseDto>(json, JsonOptions);
        return auth!.Token;
    }

    [Fact]
    public async Task GetMyTickets_Unauthorized_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/tickets/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_Authorized_Returns201()
    {
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "_create");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/tickets", new CreateTicketDto
        {
            Title = "Test Ticket",
            Description = "A ticket for testing",
            Priority = TicketPriority.Medium
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetMyTickets_AfterCreation_ReturnsTickets()
    {
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "_my");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/v1/tickets", new CreateTicketDto
        {
            Title = "My Ticket",
            Description = "Description",
            Priority = TicketPriority.Low
        });

        var response = await client.GetAsync("/api/v1/tickets/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.Single(tickets);
    }

    [Fact]
    public async Task GetById_ExistingTicket_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "_getbyid");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/v1/tickets", new CreateTicketDto
        {
            Title = "Find This",
            Description = "Description"
        });

        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>();
        var response = await client.GetAsync($"/api/v1/tickets/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistentTicket_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "_404");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/tickets/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_OwnTicket_Returns200WithDeletedTicket()
    {
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "_del");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/v1/tickets", new CreateTicketDto
        {
            Title = "Delete Me",
            Description = "Will be deleted"
        });

        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>();
        var deleteResponse = await client.DeleteAsync($"/api/v1/tickets/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deleted = await deleteResponse.Content.ReadFromJsonAsync<TicketDto>();
        Assert.NotNull(deleted);
        Assert.Equal("Delete Me", deleted.Title);
    }
}
