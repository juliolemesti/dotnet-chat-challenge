using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatChallenge.Api.Services;

public class DataMigrationService
{
  private readonly ChatDbContext _context;
  private readonly IEncryptionService _encryptionService;

  public DataMigrationService(ChatDbContext context, IEncryptionService encryptionService)
  {
    _context = context;
    _encryptionService = encryptionService;
  }

  public async Task MigrateUserDataAsync()
  {
    var users = await _context.Users.ToListAsync();
    
    foreach (var user in users)
    {
      // Check if data is already encrypted (simple check: encrypted data should be base64)
      if (!IsBase64String(user.Email))
      {
        user.Email = _encryptionService.Encrypt(user.Email);
        _context.Entry(user).Property(u => u.Email).IsModified = true;
      }
      
      if (!IsBase64String(user.UserName))
      {
        user.UserName = _encryptionService.Encrypt(user.UserName);
        _context.Entry(user).Property(u => u.UserName).IsModified = true;
      }
    }
    
    if (_context.ChangeTracker.HasChanges())
    {
      await _context.SaveChangesAsync();
    }
  }

  private static bool IsBase64String(string s)
  {
    if (string.IsNullOrEmpty(s))
      return false;
      
    try
    {
      Convert.FromBase64String(s);
      return true;
    }
    catch
    {
      return false;
    }
  }
}
