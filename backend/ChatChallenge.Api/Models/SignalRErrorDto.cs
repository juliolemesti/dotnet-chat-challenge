namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for error notifications via SignalR
/// </summary>
public class SignalRErrorDto
{
  public string Message { get; set; } = string.Empty;
  public string? Code { get; set; }
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
