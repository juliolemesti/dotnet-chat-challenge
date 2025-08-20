using Microsoft.Extensions.Logging;
using ChatChallenge.Application.DTOs;
using ChatChallenge.Application.Interfaces;

namespace ChatChallenge.Application.Services;

/// <summary>
/// Real implementation of stock bot service using message broker
/// </summary>
public class StockBotService : IStockBotService
{
  private readonly IMessageBrokerService _messageBroker;
  private readonly ILogger<StockBotService> _logger;

  public StockBotService(IMessageBrokerService messageBroker, ILogger<StockBotService> logger)
  {
    _messageBroker = messageBroker;
    _logger = logger;
  }

  public async Task QueueStockRequestAsync(string stockSymbol, string requestedBy, string roomId)
  {
    try
    {
      var stockRequest = new StockRequestMessage
      {
        StockSymbol = stockSymbol,
        RequestedBy = requestedBy,
        RoomId = roomId,
        RequestedAt = DateTime.UtcNow
      };

      _logger.LogInformation("Queueing stock request: {Symbol} for user {User} in room {RoomId}", 
        stockSymbol, requestedBy, roomId);

      await _messageBroker.PublishStockRequestAsync(stockRequest);
      
      _logger.LogInformation("Successfully queued stock request: {Symbol}", stockSymbol);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to queue stock request: {Symbol} for user {User} in room {RoomId}", 
        stockSymbol, requestedBy, roomId);
      throw;
    }
  }

  public bool IsValidStockSymbol(string stockSymbol)
  {
    if (string.IsNullOrWhiteSpace(stockSymbol) || stockSymbol.Length > 20)
      return false;

    return System.Text.RegularExpressions.Regex.IsMatch(stockSymbol, @"^[A-Za-z0-9.\-]+$");
  }

  public string? ExtractStockSymbol(string command)
  {
    if (string.IsNullOrEmpty(command) || !command.StartsWith("/stock=", StringComparison.OrdinalIgnoreCase))
      return null;

    var symbol = command.Substring(7).Trim().ToUpperInvariant();
    
    return IsValidStockSymbol(symbol) ? symbol : null;
  }
}
