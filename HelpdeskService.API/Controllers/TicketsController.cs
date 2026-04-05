using System.Security.Claims;
using Asp.Versioning;
using HelpdeskService.API.Helpers;
using HelpdeskService.Core.Common;
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

    private IActionResult ToActionResult<T>(ServiceResult<T> result, Func<T, IActionResult> onSuccess) =>
        result.Error switch
        {
            ServiceResultError.None => onSuccess(result.Value!),
            ServiceResultError.NotFound => NotFound(new { result.ErrorMessage }),
            ServiceResultError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { result.ErrorMessage }),
            ServiceResultError.Conflict => Conflict(new { result.ErrorMessage }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };

    /// <summary>Gets all tickets (admin view).</summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminOrAgent")]
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
        var userId = User.GetCurrentUserId();
        var tickets = await _ticketsService.GetTicketsByUserIdAsync(userId);
        return Ok(tickets);
    }

    /// <summary>Gets a specific ticket by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketsService.GetTicketByIdAsync(id);
        return ToActionResult(result, Ok);
    }

    /// <summary>Creates a new support ticket.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var userId = User.GetCurrentUserId();
        var result = await _ticketsService.CreateTicketAsync(userId, dto);
        return ToActionResult(result, ticket =>
            CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket));
    }

    /// <summary>Updates an existing ticket.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto dto)
    {
        var userId = User.GetCurrentUserId();
        var userRole = User.GetCurrentUserRole();
        var result = await _ticketsService.UpdateTicketAsync(id, userId, userRole, dto);
        return ToActionResult(result, Ok);
    }

    /// <summary>Deletes a ticket.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetCurrentUserId();
        var userRole = User.GetCurrentUserRole();
        var result = await _ticketsService.DeleteTicketAsync(id, userId, userRole);
        return ToActionResult(result, Ok);
    }

    /// <summary>Adds a comment to a ticket.</summary>
    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
    {
        var userId = User.GetCurrentUserId();
        var result = await _ticketsService.AddCommentAsync(id, userId, dto);
        return ToActionResult(result, comment =>
            CreatedAtAction(nameof(GetById), new { id }, comment));
    }
}
