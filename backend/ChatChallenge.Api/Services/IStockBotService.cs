using ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Services;

/// <summary>
/// Interface for stock bot service (preparation for future RabbitMQ integration)
/// </summary>
public interface IStockBotService
{
  /// <summary>
  /// Queue a stock quote request (future RabbitMQ implementation)
  /// </summary>
  /// <param name="stockSymbol">The stock symbol to query</param>
  /// <param name="requestedBy">The user requesting the stock quote</param>
  /// <param name="roomId">The room where the request originated</param>
  /// <returns>Task representing the async operation</returns>
  Task QueueStockRequestAsync(string stockSymbol, string requestedBy, string roomId);

  /// <summary>
  /// Validate if a stock symbol is in correct format
  /// </summary>
  /// <param name="stockSymbol">The stock symbol to validate</param>
  /// <returns>True if the symbol is valid</returns>
  bool IsValidStockSymbol(string stockSymbol);

  /// <summary>
  /// Extract stock symbol from command string
  /// </summary>
  /// <param name="command">The command string (e.g., "/stock=AAPL.US")</param>
  /// <returns>The extracted stock symbol or null if invalid</returns>
  string? ExtractStockSymbol(string command);
}

/// <summary>
/// Placeholder implementation of stock bot service
/// TODO: Replace with actual RabbitMQ implementation
/// </summary>
public class StockBotService : IStockBotService
{
  public Task QueueStockRequestAsync(string stockSymbol, string requestedBy, string roomId)
  {
    // TODO: Implement RabbitMQ message queuing
    // This should NOT save to the database
    // Instead, it should queue the request for processing by external bot service
    
    Console.WriteLine($"[PLACEHOLDER] Queueing stock request: {stockSymbol} for user {requestedBy} in room {roomId}");
    return Task.CompletedTask;
  }

  public bool IsValidStockSymbol(string stockSymbol)
  {
    if (string.IsNullOrWhiteSpace(stockSymbol) || stockSymbol.Length > 20)
      return false;

    // Allow alphanumeric characters, dots, and hyphens (common in stock symbols)
    return System.Text.RegularExpressions.Regex.IsMatch(stockSymbol, @"^[A-Za-z0-9.\-]+$");
  }

  public string? ExtractStockSymbol(string command)
  {
    if (!command.StartsWith("/stock=", StringComparison.OrdinalIgnoreCase))
      return null;

    var symbol = command.Substring(7).Trim().ToUpperInvariant();
    
    return IsValidStockSymbol(symbol) ? symbol : null;
  }
}
