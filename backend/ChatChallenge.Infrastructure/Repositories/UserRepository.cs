using Microsoft.EntityFrameworkCore;
using ChatChallenge.Core.Entities;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Data;

namespace ChatChallenge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
  private readonly ChatDbContext _context;
  private readonly IPasswordService _passwordService;
  private readonly IEncryptionService _encryptionService;

  public UserRepository(ChatDbContext context, IPasswordService passwordService, IEncryptionService encryptionService)
  {
    _context = context;
    _passwordService = passwordService;
    _encryptionService = encryptionService;
  }

  public async Task<User?> GetUserByEmailAsync(string email)
  {
    // Since we use random IVs, we can't encrypt the search term and compare
    // We need to get all users and decrypt their emails to find the match
    var users = await _context.Users.ToListAsync();
    
    foreach (var user in users)
    {
      var decryptedEmail = _encryptionService.Decrypt(user.Email);
      if (decryptedEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
      {
        user.Email = decryptedEmail;
        user.UserName = _encryptionService.Decrypt(user.UserName);
        return user;
      }
    }
    
    return null;
  }

  public async Task<User?> GetUserByUserNameAsync(string userName)
  {
    // Since we use random IVs, we can't encrypt the search term and compare
    // We need to get all users and decrypt their usernames to find the match
    var users = await _context.Users.ToListAsync();
    
    foreach (var user in users)
    {
      var decryptedUserName = _encryptionService.Decrypt(user.UserName);
      if (decryptedUserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
      {
        user.Email = _encryptionService.Decrypt(user.Email);
        user.UserName = decryptedUserName;
        return user;
      }
    }
    
    return null;
  }

  public async Task<User> CreateUserAsync(User user, string password)
  {
    user.CreatedAt = DateTime.UtcNow;
    user.PasswordHash = _passwordService.HashPassword(password);
    
    // Encrypt email and username before saving to database
    user.Email = _encryptionService.Encrypt(user.Email);
    user.UserName = _encryptionService.Encrypt(user.UserName);
    
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    
    // Decrypt for return value so the caller gets the original values
    user.Email = _encryptionService.Decrypt(user.Email);
    user.UserName = _encryptionService.Decrypt(user.UserName);
    
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
