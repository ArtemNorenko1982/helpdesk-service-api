using Asp.Versioning;
using HelpdeskService.API.Helpers;
using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpdeskService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registers a new user.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.IsSuccess)
            return Conflict(new { result.ErrorMessage });

        return CreatedAtAction(nameof(Register), result.Value);
    }

    /// <summary>Authenticates a user and returns a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        var user = User.Identity?.IsAuthenticated == true ? User.GetCurrentUserName() : "Anonymous";
        if (!result.IsSuccess)
            return Unauthorized(new { result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpGet]
    [Route("me")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Unauthorized();

        var response = new AuthResponseDto
        {
            Username = User.GetCurrentUserName(),
            Email = User.GetCurrentUserEmail(),
            Role = User.GetCurrentUserRole(),
        };

        return Ok(response);
    }

    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] string token)
    {
        var result = await _authService.RefreshTokenAsync(token);
        if (!result.IsSuccess)
            return Unauthorized(new { result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout() => Ok(new {Message = "Logged out successfully."});
}