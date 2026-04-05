using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;

namespace HelpdeskService.Core.Interfaces;

public interface ITicketsService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    Task<ServiceResult<TicketDto>> GetTicketByIdAsync(int ticketId);
    Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(int userId);
    Task<ServiceResult<TicketDto>> CreateTicketAsync(int userId, CreateTicketDto dto);
    Task<ServiceResult<TicketDto>> UpdateTicketAsync(int ticketId, int userId, UserRole role, UpdateTicketDto dto);
    Task<ServiceResult<TicketDto>> DeleteTicketAsync(int ticketId, int userId, UserRole role);
    Task<ServiceResult<CommentDto>> AddCommentAsync(int ticketId, int userId, CreateCommentDto dto);
}
