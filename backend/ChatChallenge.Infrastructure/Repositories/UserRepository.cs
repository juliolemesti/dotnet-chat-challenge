using Microsoft.EntityFrameworkCore;
using ChatChallenge.Core.Entities;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Data;

namespace ChatChallenge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
  private readonly ChatDbContext _context;

  public UserRepository(ChatDbContext context)
  {
    _context = context;
  }

  public async Task<User?> GetUserByEmailAsync(string email)
  {
    return await _context.Users
      .FirstOrDefaultAsync(u => u.Email == email);
  }

  public async Task<User> CreateUserAsync(User user)
  {
    user.CreatedAt = DateTime.UtcNow;
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return user;
  }

  public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
  {
    // For demo purposes - in production, use proper password hashing
    var user = await GetUserByEmailAsync(email);
    return user != null;
  }
}
