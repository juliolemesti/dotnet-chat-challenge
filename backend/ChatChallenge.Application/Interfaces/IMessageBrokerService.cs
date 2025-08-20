using ChatChallenge.Application.DTOs;

namespace ChatChallenge.Application.Interfaces;

/// <summary>
/// Interface for message broker service operations
/// </summary>
public interface IMessageBrokerService
{
  /// <summary>
  /// Publish a stock request message to the broker
  /// </summary>
  /// <param name="stockRequest">The stock request to publish</param>
  /// <returns>Task representing the async operation</returns>
  Task PublishStockRequestAsync(StockRequestMessage stockRequest);

  /// <summary>
  /// Subscribe to stock responses for a specific room
  /// </summary>
  /// <param name="roomId">The room ID to subscribe to</param>
  /// <param name="onStockResponse">Callback when stock response is received</param>
  void SubscribeToStockResponses(string roomId, Func<StockResponseMessage, Task> onStockResponse);

  /// <summary>
  /// Unsubscribe from stock responses for a specific room
  /// </summary>
  /// <param name="roomId">The room ID to unsubscribe from</param>
  void UnsubscribeFromStockResponses(string roomId);
}
