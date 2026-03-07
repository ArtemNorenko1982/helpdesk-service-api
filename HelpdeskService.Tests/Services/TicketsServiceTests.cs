using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Data.Context;
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

    private static async Task<User> SeedUserAsync(HelpdeskDbContext context, string suffix = "")
    {
        var user = new User
        {
            Username = $"testuser{suffix}",
            Email = $"test{suffix}@example.com",
            PasswordHash = "hashed",
            Role = "User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task CreateTicketAsync_ValidData_ReturnsTicketDto()
    {
        using var context = CreateInMemoryContext(nameof(CreateTicketAsync_ValidData_ReturnsTicketDto));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        var dto = new CreateTicketDto
        {
            Title = "Test Ticket",
            Description = "This is a test ticket",
            Priority = TicketPriority.High
        };

        var result = await service.CreateTicketAsync(user.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("Test Ticket", result.Title);
        Assert.Equal("High", result.Priority);
        Assert.Equal("Open", result.Status);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task GetAllTicketsAsync_ReturnsAllTickets()
    {
        using var context = CreateInMemoryContext(nameof(GetAllTicketsAsync_ReturnsAllTickets));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        await service.CreateTicketAsync(user.Id, new CreateTicketDto { Title = "T1", Description = "D1" });
        await service.CreateTicketAsync(user.Id, new CreateTicketDto { Title = "T2", Description = "D2" });

        var tickets = await service.GetAllTicketsAsync();

        Assert.Equal(2, tickets.Count());
    }

    [Fact]
    public async Task GetTicketByIdAsync_ExistingTicket_ReturnsDto()
    {
        using var context = CreateInMemoryContext(nameof(GetTicketByIdAsync_ExistingTicket_ReturnsDto));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        var created = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "FindMe",
            Description = "Description"
        });

        var result = await service.GetTicketByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("FindMe", result.Title);
    }

    [Fact]
    public async Task GetTicketByIdAsync_NonExistentTicket_ReturnsNull()
    {
        using var context = CreateInMemoryContext(nameof(GetTicketByIdAsync_NonExistentTicket_ReturnsNull));
        var service = new TicketsService(context);

        var result = await service.GetTicketByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTicketsByUserIdAsync_ReturnsOnlyUserTickets()
    {
        using var context = CreateInMemoryContext(nameof(GetTicketsByUserIdAsync_ReturnsOnlyUserTickets));
        var user1 = await SeedUserAsync(context, "1");
        var user2 = await SeedUserAsync(context, "2");
        var service = new TicketsService(context);

        await service.CreateTicketAsync(user1.Id, new CreateTicketDto { Title = "U1T1", Description = "D" });
        await service.CreateTicketAsync(user1.Id, new CreateTicketDto { Title = "U1T2", Description = "D" });
        await service.CreateTicketAsync(user2.Id, new CreateTicketDto { Title = "U2T1", Description = "D" });

        var user1Tickets = await service.GetTicketsByUserIdAsync(user1.Id);

        Assert.Equal(2, user1Tickets.Count());
        Assert.All(user1Tickets, t => Assert.Equal(user1.Id, t.UserId));
    }

    [Fact]
    public async Task UpdateTicketAsync_ValidRequest_UpdatesAndReturnsDto()
    {
        using var context = CreateInMemoryContext(nameof(UpdateTicketAsync_ValidRequest_UpdatesAndReturnsDto));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        var ticket = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "Original",
            Description = "Original Desc"
        });

        var updateDto = new UpdateTicketDto
        {
            Title = "Updated Title",
            Status = TicketStatus.InProgress
        };

        var result = await service.UpdateTicketAsync(ticket.Id, user.Id, updateDto);

        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task UpdateTicketAsync_WrongUser_ReturnsNull()
    {
        using var context = CreateInMemoryContext(nameof(UpdateTicketAsync_WrongUser_ReturnsNull));
        var user1 = await SeedUserAsync(context, "1");
        var user2 = await SeedUserAsync(context, "2");
        var service = new TicketsService(context);

        var ticket = await service.CreateTicketAsync(user1.Id, new CreateTicketDto
        {
            Title = "Owned by user1",
            Description = "D"
        });

        var result = await service.UpdateTicketAsync(ticket.Id, user2.Id, new UpdateTicketDto { Title = "Hijack" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTicketAsync_ValidRequest_ReturnsTrueAndDeletes()
    {
        using var context = CreateInMemoryContext(nameof(DeleteTicketAsync_ValidRequest_ReturnsTrueAndDeletes));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        var ticket = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "To Delete",
            Description = "D"
        });

        var deleted = await service.DeleteTicketAsync(ticket.Id, user.Id);
        var found = await service.GetTicketByIdAsync(ticket.Id);

        Assert.True(deleted);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteTicketAsync_WrongUser_ReturnsFalse()
    {
        using var context = CreateInMemoryContext(nameof(DeleteTicketAsync_WrongUser_ReturnsFalse));
        var user1 = await SeedUserAsync(context, "1");
        var user2 = await SeedUserAsync(context, "2");
        var service = new TicketsService(context);

        var ticket = await service.CreateTicketAsync(user1.Id, new CreateTicketDto
        {
            Title = "Protected",
            Description = "D"
        });

        var result = await service.DeleteTicketAsync(ticket.Id, user2.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task AddCommentAsync_ValidRequest_ReturnsCommentDto()
    {
        using var context = CreateInMemoryContext(nameof(AddCommentAsync_ValidRequest_ReturnsCommentDto));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        var ticket = await service.CreateTicketAsync(user.Id, new CreateTicketDto
        {
            Title = "Commentable",
            Description = "D"
        });

        var comment = await service.AddCommentAsync(ticket.Id, user.Id, new CreateCommentDto
        {
            Content = "This is a comment"
        });

        Assert.NotNull(comment);
        Assert.Equal("This is a comment", comment.Content);
        Assert.Equal(user.Id, comment.UserId);
    }

    [Fact]
    public async Task AddCommentAsync_NonExistentTicket_ReturnsNull()
    {
        using var context = CreateInMemoryContext(nameof(AddCommentAsync_NonExistentTicket_ReturnsNull));
        var user = await SeedUserAsync(context);
        var service = new TicketsService(context);

        var comment = await service.AddCommentAsync(999, user.Id, new CreateCommentDto
        {
            Content = "Comment on missing ticket"
        });

        Assert.Null(comment);
    }
}
