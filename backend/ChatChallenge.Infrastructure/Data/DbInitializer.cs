using ChatChallenge.Core.Entities;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Data;

namespace ChatChallenge.Infrastructure.Data;

public static class DbInitializer
{
  public static void Initialize(ChatDbContext context, IPasswordService passwordService, IEncryptionService encryptionService)
  {
    context.Database.EnsureCreated();

    if (context.ChatRooms.Any()) { return; }

    var defaultRooms = new[]
    {
      new ChatRoom
      {
        Name = "General",
        CreatedAt = DateTime.UtcNow
      },
      new ChatRoom
      {
        Name = "Random",
        CreatedAt = DateTime.UtcNow
      }
    };

    context.ChatRooms.AddRange(defaultRooms);

    var demoUsers = new[]
    {
      new User
      {
        Email = encryptionService.Encrypt("demo@chat.com"),
        UserName = encryptionService.Encrypt("DemoUser"),
        PasswordHash = passwordService.HashPassword("test123"),
        CreatedAt = DateTime.UtcNow
      },
      new User
      {
        Email = encryptionService.Encrypt("test@chat.com"),
        UserName = encryptionService.Encrypt("TestUser"),
        PasswordHash = passwordService.HashPassword("test123"),
        CreatedAt = DateTime.UtcNow
      }
    };

    context.Users.AddRange(demoUsers);
    context.SaveChanges();

    var welcomeMessages = new[]
    {
      new ChatMessage
      {
        Content = "Welcome to the Chat Challenge! 🎉",
        UserName = "System",
        ChatRoomId = defaultRooms[0].Id,
        CreatedAt = DateTime.UtcNow.AddMinutes(-10),
        IsStockBot = false
      },
      new ChatMessage
      {
        Content = "Try typing /stock=AAPL.US to get stock quotes!",
        UserName = "System",
        ChatRoomId = defaultRooms[0].Id,
        CreatedAt = DateTime.UtcNow.AddMinutes(-9),
        IsStockBot = false
      }
    };

    context.ChatMessages.AddRange(welcomeMessages);
    context.SaveChanges();
  }
}
