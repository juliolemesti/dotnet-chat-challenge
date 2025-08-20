namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for chat messages sent via SignalR
/// </summary>
public class SignalRMessageDto
{
  public int Id { get; set; }
  public string Content { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public int RoomId { get; set; }
  public DateTime CreatedAt { get; set; }
  public bool IsStockBot { get; set; }
}
