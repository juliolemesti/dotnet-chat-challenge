namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for user presence notifications via SignalR
/// </summary>
public class SignalRUserPresenceDto
{
  public string UserName { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
