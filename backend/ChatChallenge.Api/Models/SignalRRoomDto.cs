namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for room information sent via SignalR
/// </summary>
public class SignalRRoomDto
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public int MemberCount { get; set; } = 0;
}
