using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskService.Data.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly HelpdeskDbContext _context;

    public TicketRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync() =>
        await _context.Tickets
            .Include(t => t.User)
            // Comments are lazy-loaded on demand (virtual navigation property + UseLazyLoadingProxies)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public Task<Ticket?> FindByIdAsync(int id) =>
        _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId) =>
        await _context.Tickets
            .Include(t => t.User)
            // Comments are lazy-loaded on demand (virtual navigation property + UseLazyLoadingProxies)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return (await FindByIdAsync(ticket.Id))!;
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        // The entity is tracked by the context (FindByIdAsync uses tracking by default).
        // Any property changes made before this call will be persisted on SaveChangesAsync.
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Ticket ticket)
    {
        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }
}
