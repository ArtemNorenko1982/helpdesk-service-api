using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;

namespace HelpdeskService.Data.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.FindByIdAsync(id);

        return user is null
            ? ServiceResult<UserDto>.NotFound($"User with ID {id} not found.")
            : ServiceResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ServiceResult<UserDto>> ChangeRoleAsync(int userId, UserRole newRole)
    {
        var user = await _userRepository.FindByIdAsync(userId);

        if (user is null)
            return ServiceResult<UserDto>.NotFound($"User with ID {userId} not found.");

        user.Role = newRole;
        await _userRepository.UpdateAsync(user);

        return ServiceResult<UserDto>.Success(MapToDto(user));
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role,
        CreatedAt = user.CreatedAt
    };
}