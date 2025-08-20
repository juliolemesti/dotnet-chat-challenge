namespace ChatChallenge.Api.Models;

/// <summary>
/// Message for stock responses sent through the message broker
/// </summary>
public class StockResponseMessage
{
  public string StockSymbol { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public string FormattedMessage { get; set; } = string.Empty;
  public string RequestedBy { get; set; } = string.Empty;
  public DateTime ResponseAt { get; set; } = DateTime.UtcNow;
  public bool IsError { get; set; } = false;
  public string? ErrorMessage { get; set; }
}
