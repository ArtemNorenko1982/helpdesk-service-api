using HelpdeskService.Core.Entities;

namespace HelpdeskService.Core.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(int id);
    Task<User> AddAsync(User user);
}
