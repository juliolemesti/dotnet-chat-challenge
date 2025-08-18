using Microsoft.EntityFrameworkCore;
using ChatChallenge.Core.Entities;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Data;

namespace ChatChallenge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
  private readonly ChatDbContext _context;
  private readonly IPasswordService _passwordService;

  public UserRepository(ChatDbContext context, IPasswordService passwordService)
  {
    _context = context;
    _passwordService = passwordService;
  }

  public async Task<User?> GetUserByEmailAsync(string email)
  {
    return await _context.Users
      .FirstOrDefaultAsync(u => u.Email == email);
  }

  public async Task<User?> GetUserByUserNameAsync(string userName)
  {
    return await _context.Users
      .FirstOrDefaultAsync(u => u.UserName == userName);
  }

  public async Task<User> CreateUserAsync(User user, string password)
  {
    user.CreatedAt = DateTime.UtcNow;
    user.PasswordHash = _passwordService.HashPassword(password);
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return user;
  }

  public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
  {
    var user = await GetUserByEmailAsync(email);
    if (user == null)
    {
      return false;
    }

    return _passwordService.VerifyPassword(password, user.PasswordHash);
  }
}
