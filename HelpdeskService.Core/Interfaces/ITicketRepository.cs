using HelpdeskService.Core.Entities;

namespace HelpdeskService.Core.Interfaces;

public interface ITicketRepository
{
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task<Ticket?> FindByIdAsync(int id);
    Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId);
    Task<Ticket> AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
    Task RemoveAsync(Ticket ticket);
    Task<Comment> AddCommentAsync(Comment comment);
}
