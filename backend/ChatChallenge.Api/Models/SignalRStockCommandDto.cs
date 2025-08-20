namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for stock bot commands (preparation for RabbitMQ integration)
/// </summary>
public class SignalRStockCommandDto
{
  public string StockSymbol { get; set; } = string.Empty;
  public string RequestedBy { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
  public string? Status { get; set; } // "Pending", "Processing", "Completed", "Failed"
}
