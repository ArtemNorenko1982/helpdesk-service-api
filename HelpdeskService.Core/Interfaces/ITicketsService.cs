using HelpdeskService.Core.DTOs;

namespace HelpdeskService.Core.Interfaces;

public interface ITicketsService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    Task<TicketDto?> GetTicketByIdAsync(int id);
    Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(int userId);
    Task<TicketDto> CreateTicketAsync(int userId, CreateTicketDto dto);
    Task<TicketDto?> UpdateTicketAsync(int id, int userId, UpdateTicketDto dto);
    Task<bool> DeleteTicketAsync(int id, int userId);
    Task<CommentDto?> AddCommentAsync(int ticketId, int userId, CreateCommentDto dto);
}
