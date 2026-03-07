using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;

namespace HelpdeskService.Data.Services;

public class TicketMapper : ITicketMapper
{
    public TicketDto MapToDto(Ticket ticket) => new()
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
        Comments = ticket.Comments.Select(MapCommentToDto)
    };

    public CommentDto MapCommentToDto(Comment comment) => new()
    {
        Id = comment.Id,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UserId = comment.UserId,
        Username = comment.User?.Username ?? string.Empty
    };
}
