using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;

namespace HelpdeskService.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<ServiceResult<UserDto>> GetUserByIdAsync(int id);
    Task<ServiceResult<UserDto>> ChangeRoleAsync(int userId, UserRole newRole);
}