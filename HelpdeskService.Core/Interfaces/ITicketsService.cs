using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;

namespace HelpdeskService.Core.Interfaces;

public interface ITicketsService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    Task<ServiceResult<TicketDto>> GetTicketByIdAsync(int id);
    Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(int userId);
    Task<ServiceResult<TicketDto>> CreateTicketAsync(int userId, CreateTicketDto dto);
    Task<ServiceResult<TicketDto>> UpdateTicketAsync(int id, int userId, UpdateTicketDto dto);
    Task<ServiceResult<TicketDto>> DeleteTicketAsync(int id, int userId);
    Task<ServiceResult<CommentDto>> AddCommentAsync(int ticketId, int userId, CreateCommentDto dto);
}
