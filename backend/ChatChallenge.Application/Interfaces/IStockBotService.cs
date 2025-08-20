using ChatChallenge.Application.DTOs;

namespace ChatChallenge.Application.Interfaces;

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
