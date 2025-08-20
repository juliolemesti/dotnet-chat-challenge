namespace ChatChallenge.Application.DTOs;

/// <summary>
/// DTO for typing indicators via SignalR (future enhancement)
/// </summary>
public class SignalRTypingDto
{
  public string UserName { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public bool IsTyping { get; set; }
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
