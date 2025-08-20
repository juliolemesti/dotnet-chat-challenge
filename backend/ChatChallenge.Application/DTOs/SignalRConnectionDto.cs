namespace ChatChallenge.Application.DTOs;

/// <summary>
/// DTO for connection status via SignalR
/// </summary>
public class SignalRConnectionDto
{
  public string UserName { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}
