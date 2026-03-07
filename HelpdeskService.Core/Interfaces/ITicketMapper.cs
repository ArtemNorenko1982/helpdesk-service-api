using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;

namespace HelpdeskService.Core.Interfaces;

public interface ITicketMapper
{
    TicketDto MapToDto(Ticket ticket);
    CommentDto MapCommentToDto(Comment comment);
}
