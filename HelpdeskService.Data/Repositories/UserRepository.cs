using HelpdeskService.Core.Entities;
using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskService.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HelpdeskDbContext _context;

    public UserRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsByEmailAsync(string email) =>
        _context.Users.AnyAsync(u => u.Email == email);

    public Task<User?> FindByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> FindByIdAsync(int id) =>
        _context.Users.FindAsync(id).AsTask();

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
