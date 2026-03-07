using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;

namespace HelpdeskService.Data.Services;

public class TicketsService : ITicketsService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketMapper _mapper;

    public TicketsService(
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        ITicketMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
    {
        var tickets = await _ticketRepository.GetAllAsync();
        return tickets.Select(_mapper.MapToDto);
    }

    public async Task<ServiceResult<TicketDto>> GetTicketByIdAsync(int id)
    {
        var ticket = await _ticketRepository.FindByIdAsync(id);

        return ticket is null
            ? ServiceResult<TicketDto>.NotFound($"Ticket with ID {id} not found.")
            : ServiceResult<TicketDto>.Success(_mapper.MapToDto(ticket));
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(int userId)
    {
        var tickets = await _ticketRepository.GetByUserIdAsync(userId);
        return tickets.Select(_mapper.MapToDto);
    }

    public async Task<ServiceResult<TicketDto>> CreateTicketAsync(int userId, CreateTicketDto dto)
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

        var created = await _ticketRepository.AddAsync(ticket);
        return ServiceResult<TicketDto>.Success(_mapper.MapToDto(created));
    }

    public async Task<ServiceResult<TicketDto>> UpdateTicketAsync(int id, int userId, UpdateTicketDto dto)
    {
        var ticket = await _ticketRepository.FindByIdAsync(id);

        if (ticket is null)
            return ServiceResult<TicketDto>.NotFound($"Ticket with ID {id} not found.");

        if (ticket.UserId != userId)
            return ServiceResult<TicketDto>.Forbidden("You do not have permission to update this ticket.");

        if (dto.Title is not null) ticket.Title = dto.Title;
        if (dto.Description is not null) ticket.Description = dto.Description;
        if (dto.Status.HasValue) ticket.Status = dto.Status.Value;
        if (dto.Priority.HasValue) ticket.Priority = dto.Priority.Value;

        await _ticketRepository.UpdateAsync(ticket);

        return ServiceResult<TicketDto>.Success(_mapper.MapToDto(ticket));
    }

    public async Task<ServiceResult<TicketDto>> DeleteTicketAsync(int id, int userId)
    {
        var ticket = await _ticketRepository.FindByIdAsync(id);

        if (ticket is null)
            return ServiceResult<TicketDto>.NotFound($"Ticket with ID {id} not found.");

        if (ticket.UserId != userId)
            return ServiceResult<TicketDto>.Forbidden("You do not have permission to delete this ticket.");

        var deleted = _mapper.MapToDto(ticket);
        await _ticketRepository.RemoveAsync(ticket);

        return ServiceResult<TicketDto>.Success(deleted);
    }

    public async Task<ServiceResult<CommentDto>> AddCommentAsync(int ticketId, int userId, CreateCommentDto dto)
    {
        var ticket = await _ticketRepository.FindByIdAsync(ticketId);

        if (ticket is null)
            return ServiceResult<CommentDto>.NotFound($"Ticket with ID {ticketId} not found.");

        var user = await _userRepository.FindByIdAsync(userId);

        if (user is null)
            return ServiceResult<CommentDto>.NotFound($"User with ID {userId} not found.");

        var comment = new Comment
        {
            Content = dto.Content,
            TicketId = ticketId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        comment = await _ticketRepository.AddCommentAsync(comment);
        comment.User = user;

        return ServiceResult<CommentDto>.Success(_mapper.MapCommentToDto(comment));
    }
}
