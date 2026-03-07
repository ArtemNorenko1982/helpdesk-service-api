using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Data.Context;
using HelpdeskService.Data.Repositories;
using HelpdeskService.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskService.Tests.Services;

public class TicketsServiceTests
{
    private static HelpdeskDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new HelpdeskDbContext(options);
    }

    private static TicketsService CreateService(HelpdeskDbContext context) =>
        new(new TicketRepository(context), new UserRepository(context), new TicketMapper());

    private static async Task<User> SeedUserAsync(HelpdeskDbContext context, string suffix = "")
    {
        var user = new User
        {
            Username = $"testuser{suffix}",
            Email = $"test{suffix}@example.com",
            PasswordHash = "hashed",
            Role = UserRole.User
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task CreateTicketAsync_ValidData_ReturnsSuccess()
    {
        using var context = CreateInMemoryContext(nameof(CreateTicketAsync_ValidData_ReturnsSuccess));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        var result = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "Test Ticket",
            Description = "This is a test ticket",
            Priority = TicketPriority.High
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Ticket", result.Value.Title);
        Assert.Equal("High", result.Value.Priority);
        Assert.Equal("Open", result.Value.Status);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task GetAllTicketsAsync_ReturnsAllTickets()
    {
        using var context = CreateInMemoryContext(nameof(GetAllTicketsAsync_ReturnsAllTickets));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        await service.CreateTicketAsync(user.Id, new CreateTicketDto { Title = "T1", Description = "D1" });
        await service.CreateTicketAsync(user.Id, new CreateTicketDto { Title = "T2", Description = "D2" });

        var tickets = await service.GetAllTicketsAsync();

        Assert.Equal(2, tickets.Count());
    }

    [Fact]
    public async Task GetTicketByIdAsync_ExistingTicket_ReturnsSuccess()
    {
        using var context = CreateInMemoryContext(nameof(GetTicketByIdAsync_ExistingTicket_ReturnsSuccess));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        var created = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "FindMe",
            Description = "Description"
        });

        var result = await service.GetTicketByIdAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("FindMe", result.Value!.Title);
    }

    [Fact]
    public async Task GetTicketByIdAsync_NonExistentTicket_ReturnsNotFound()
    {
        using var context = CreateInMemoryContext(nameof(GetTicketByIdAsync_NonExistentTicket_ReturnsNotFound));
        var service = CreateService(context);

        var result = await service.GetTicketByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task GetTicketsByUserIdAsync_ReturnsOnlyUserTickets()
    {
        using var context = CreateInMemoryContext(nameof(GetTicketsByUserIdAsync_ReturnsOnlyUserTickets));
        var user1 = await SeedUserAsync(context, "1");
        var user2 = await SeedUserAsync(context, "2");
        var service = CreateService(context);

        await service.CreateTicketAsync(user1.Id, new CreateTicketDto { Title = "U1T1", Description = "D" });
        await service.CreateTicketAsync(user1.Id, new CreateTicketDto { Title = "U1T2", Description = "D" });
        await service.CreateTicketAsync(user2.Id, new CreateTicketDto { Title = "U2T1", Description = "D" });

        var user1Tickets = await service.GetTicketsByUserIdAsync(user1.Id);

        Assert.Equal(2, user1Tickets.Count());
        Assert.All(user1Tickets, t => Assert.Equal(user1.Id, t.UserId));
    }

    [Fact]
    public async Task UpdateTicketAsync_ValidRequest_ReturnsSuccess()
    {
        using var context = CreateInMemoryContext(nameof(UpdateTicketAsync_ValidRequest_ReturnsSuccess));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        var created = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "Original",
            Description = "Original Desc"
        });

        var result = await service.UpdateTicketAsync(created.Value!.Id, user.Id, new UpdateTicketDto
        {
            Title = "Updated Title",
            Status = TicketStatus.InProgress
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Title", result.Value!.Title);
        Assert.Equal("InProgress", result.Value.Status);
    }

    [Fact]
    public async Task UpdateTicketAsync_WrongUser_ReturnsForbidden()
    {
        using var context = CreateInMemoryContext(nameof(UpdateTicketAsync_WrongUser_ReturnsForbidden));
        var user1 = await SeedUserAsync(context, "1");
        var user2 = await SeedUserAsync(context, "2");
        var service = CreateService(context);

        var created = await service.CreateTicketAsync(user1.Id, new CreateTicketDto
        {
            Title = "Owned by user1",
            Description = "D"
        });

        var result = await service.UpdateTicketAsync(created.Value!.Id, user2.Id, new UpdateTicketDto
        {
            Title = "Hijack"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.Forbidden, result.Error);
    }

    [Fact]
    public async Task DeleteTicketAsync_ValidRequest_ReturnsSuccessAndDeletes()
    {
        using var context = CreateInMemoryContext(nameof(DeleteTicketAsync_ValidRequest_ReturnsSuccessAndDeletes));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        var created = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "To Delete",
            Description = "D"
        });

        var deleteResult = await service.DeleteTicketAsync(created.Value!.Id, user.Id);
        var getResult = await service.GetTicketByIdAsync(created.Value.Id);

        Assert.True(deleteResult.IsSuccess);
        Assert.Equal("To Delete", deleteResult.Value!.Title);
        Assert.False(getResult.IsSuccess);
        Assert.Equal(ServiceResultError.NotFound, getResult.Error);
    }

    [Fact]
    public async Task DeleteTicketAsync_WrongUser_ReturnsForbidden()
    {
        using var context = CreateInMemoryContext(nameof(DeleteTicketAsync_WrongUser_ReturnsForbidden));
        var user1 = await SeedUserAsync(context, "1");
        var user2 = await SeedUserAsync(context, "2");
        var service = CreateService(context);

        var created = await service.CreateTicketAsync(user1.Id, new CreateTicketDto
        {
            Title = "Protected",
            Description = "D"
        });

        var result = await service.DeleteTicketAsync(created.Value!.Id, user2.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.Forbidden, result.Error);
    }

    [Fact]
    public async Task AddCommentAsync_ValidRequest_ReturnsSuccess()
    {
        using var context = CreateInMemoryContext(nameof(AddCommentAsync_ValidRequest_ReturnsSuccess));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        var ticket = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "Commentable",
            Description = "D"
        });

        var result = await service.AddCommentAsync(ticket.Value!.Id, user.Id, new CreateCommentDto
        {
            Content = "This is a comment"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("This is a comment", result.Value.Content);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task AddCommentAsync_NonExistentTicket_ReturnsNotFound()
    {
        using var context = CreateInMemoryContext(nameof(AddCommentAsync_NonExistentTicket_ReturnsNotFound));
        var user = await SeedUserAsync(context);
        var service = CreateService(context);

        var result = await service.AddCommentAsync(999, user.Id, new CreateCommentDto
        {
            Content = "Comment on missing ticket"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultError.NotFound, result.Error);
    }
}
