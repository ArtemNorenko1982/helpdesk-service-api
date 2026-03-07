using HelpdeskService.Core.Entities;

namespace HelpdeskService.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}
