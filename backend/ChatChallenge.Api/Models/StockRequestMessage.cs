namespace ChatChallenge.Api.Models;

/// <summary>
/// Message for stock requests sent through the message broker
/// </summary>
public class StockRequestMessage
{
  public string StockSymbol { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public string RequestedBy { get; set; } = string.Empty;
  public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
