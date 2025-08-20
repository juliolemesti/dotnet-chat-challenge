namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for room statistics via SignalR
/// </summary>
public class SignalRRoomStatsDto
{
  public int RoomId { get; set; }
  public int OnlineUsers { get; set; }
  public int MessageCount { get; set; }
  public DateTime LastActivity { get; set; }
}
