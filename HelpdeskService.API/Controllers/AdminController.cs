using Asp.Versioning;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Gets all users.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>Gets a specific user by ID.</summary>
    [HttpGet("users/{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { result.ErrorMessage });
    }

    /// <summary>Changes a user's role.</summary>
    [HttpPut("users/{id:int}/role")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleDto dto)
    {
        var result = await _userService.ChangeRoleAsync(id, dto.Role);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { result.ErrorMessage });
    }
}