namespace ChatChallenge.Core.Entities;

public class ChatRoom
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}