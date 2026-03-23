using Asp.Versioning;
using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        var username = User.Identity.Name;
        var response = new AuthResponseDto
        {
            Username = username,
            Email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
            Role = Enum.TryParse<UserRole>(User.Claims.FirstOrDefault(c => c.Type == "role")?.Value, out var role) ? role : UserRole.User,
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
    public IActionResult Logout()
    {
        // For JWT, logout is typically handled on the client side by deleting the token.
        // Optionally, you could implement token blacklisting here.
        return NoContent();
    }
}