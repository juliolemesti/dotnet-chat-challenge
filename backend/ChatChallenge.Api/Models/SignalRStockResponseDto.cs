namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for stock bot responses
/// </summary>
public class SignalRStockResponseDto
{
  public string StockSymbol { get; set; } = string.Empty;
  public decimal? Price { get; set; }
  public string FormattedMessage { get; set; } = string.Empty;
  public string RequestedBy { get; set; } = string.Empty;
  public DateTime ResponseAt { get; set; } = DateTime.UtcNow;
  public bool IsError { get; set; } = false;
  public string? ErrorMessage { get; set; }
}
