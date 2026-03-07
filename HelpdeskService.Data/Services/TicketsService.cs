using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskService.Data.Services;

public class TicketsService : ITicketsService
{
    private readonly HelpdeskDbContext _context;

    public TicketsService(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
    {
        var tickets = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto?> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        return ticket is null ? null : MapToDto(ticket);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(int userId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto> CreateTicketAsync(int userId, CreateTicketDto dto)
    {
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = TicketStatus.Open,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return (await GetTicketByIdAsync(ticket.Id))!;
    }

    public async Task<TicketDto?> UpdateTicketAsync(int id, int userId, UpdateTicketDto dto)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
            return null;

        if (ticket.UserId != userId)
            return null;

        if (dto.Title is not null) ticket.Title = dto.Title;
        if (dto.Description is not null) ticket.Description = dto.Description;
        if (dto.Status.HasValue) ticket.Status = dto.Status.Value;
        if (dto.Priority.HasValue) ticket.Priority = dto.Priority.Value;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticket.Id);
    }

    public async Task<bool> DeleteTicketAsync(int id, int userId)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null || ticket.UserId != userId)
            return false;

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CommentDto?> AddCommentAsync(int ticketId, int userId, CreateCommentDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket is null)
            return null;

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            return null;

        var comment = new Comment
        {
            Content = dto.Content,
            TicketId = ticketId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            Username = user.Username
        };
    }

    private static TicketDto MapToDto(Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status.ToString(),
        Priority = ticket.Priority.ToString(),
        CreatedAt = ticket.CreatedAt,
        UpdatedAt = ticket.UpdatedAt,
        UserId = ticket.UserId,
        Username = ticket.User?.Username ?? string.Empty,
        Comments = ticket.Comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UserId = c.UserId,
            Username = c.User?.Username ?? string.Empty
        })
    };
}
