using HelpdeskService.Core.Common;
using HelpdeskService.Core.DTOs;
using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;

namespace HelpdeskService.Data.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.ExistsByEmailAsync(dto.Email))
            return ServiceResult<AuthResponseDto>.Conflict("A user with this email already exists.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = UserRole.Guest,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return ServiceResult<AuthResponseDto>.Success(BuildAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            return ServiceResult<AuthResponseDto>.Unauthorized("Invalid email or password.");

        return ServiceResult<AuthResponseDto>.Success(BuildAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(string token)
        {
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);
        var email = principal;
        if (email is null)
            return ServiceResult<AuthResponseDto>.Unauthorized("Invalid token.");
        var user = await _userRepository.FindByEmailAsync(email);
        if (user is null)
            return ServiceResult<AuthResponseDto>.Unauthorized("User not found.");
        return ServiceResult<AuthResponseDto>.Success(BuildAuthResponse(user));
    }


    private AuthResponseDto BuildAuthResponse(User user)
    {
        var token = _tokenService.GenerateToken(user, out var expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}
