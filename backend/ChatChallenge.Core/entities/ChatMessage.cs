namespace ChatChallenge.Core.Entities;

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