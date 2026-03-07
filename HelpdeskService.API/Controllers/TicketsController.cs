using System.Security.Claims;
using Asp.Versioning;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketsService _ticketsService;

    public TicketsController(ITicketsService ticketsService)
    {
        _ticketsService = ticketsService;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found."));

    /// <summary>Gets all tickets (admin view).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _ticketsService.GetAllTicketsAsync();
        return Ok(tickets);
    }

    /// <summary>Gets tickets for the current user.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = GetCurrentUserId();
        var tickets = await _ticketsService.GetTicketsByUserIdAsync(userId);
        return Ok(tickets);
    }

    /// <summary>Gets a specific ticket by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _ticketsService.GetTicketByIdAsync(id);
        if (ticket is null)
            return NotFound(new { Message = $"Ticket with ID {id} not found." });

        return Ok(ticket);
    }

    /// <summary>Creates a new support ticket.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var userId = GetCurrentUserId();
        var ticket = await _ticketsService.CreateTicketAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    /// <summary>Updates an existing ticket.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto dto)
    {
        var userId = GetCurrentUserId();
        var ticket = await _ticketsService.UpdateTicketAsync(id, userId, dto);
        if (ticket is null)
            return NotFound(new { Message = $"Ticket with ID {id} not found or access denied." });

        return Ok(ticket);
    }

    /// <summary>Deletes a ticket.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var deleted = await _ticketsService.DeleteTicketAsync(id, userId);
        if (!deleted)
            return NotFound(new { Message = $"Ticket with ID {id} not found or access denied." });

        return NoContent();
    }

    /// <summary>Adds a comment to a ticket.</summary>
    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
    {
        var userId = GetCurrentUserId();
        var comment = await _ticketsService.AddCommentAsync(id, userId, dto);
        if (comment is null)
            return NotFound(new { Message = $"Ticket with ID {id} not found." });

        return CreatedAtAction(nameof(GetById), new { id }, comment);
    }
}
