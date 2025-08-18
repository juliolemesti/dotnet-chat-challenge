namespace ChatChallenge.Core.Entities;

public class User
{
  public int Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}

public class ChatRoom
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage
{
  public int Id { get; set; }
  public string Content { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public int ChatRoomId { get; set; }
  public ChatRoom ChatRoom { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public bool IsStockBot { get; set; } = false;
}
