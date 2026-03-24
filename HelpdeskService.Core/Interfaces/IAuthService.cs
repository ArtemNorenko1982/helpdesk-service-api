using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;

namespace HelpdeskService.Core.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(string token);
}
