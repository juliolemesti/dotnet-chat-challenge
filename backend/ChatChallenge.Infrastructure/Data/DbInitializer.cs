using ChatChallenge.Core.Entities;
using ChatChallenge.Infrastructure.Data;

namespace ChatChallenge.Infrastructure.Data;

public static class DbInitializer
{
  public static void Initialize(ChatDbContext context)
  {
    context.Database.EnsureCreated();

    // Check if database has been seeded
    if (context.ChatRooms.Any())
    {
      return; // DB has been seeded
    }

    // Seed default chat rooms
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

    // Seed demo users
    var demoUsers = new[]
    {
      new User
      {
        Email = "demo@chat.com",
        UserName = "DemoUser",
        CreatedAt = DateTime.UtcNow
      },
      new User
      {
        Email = "test@chat.com",
        UserName = "TestUser",
        CreatedAt = DateTime.UtcNow
      }
    };

    context.Users.AddRange(demoUsers);
    context.SaveChanges();

    // Seed some initial messages
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
