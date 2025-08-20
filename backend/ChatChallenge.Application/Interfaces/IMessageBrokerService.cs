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
  /// <param name="request">The stock request to publish</param>
  /// <returns>Task representing the async operation</returns>
  Task PublishStockRequestAsync(StockRequestMessage request);

  /// <summary>
  /// Consume a stock request message from the broker
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The consumed stock request message</returns>
  Task<StockRequestMessage> ConsumeStockRequestAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Publish a stock response message to the broker
  /// </summary>
  /// <param name="response">The stock response to publish</param>
  /// <returns>Task representing the async operation</returns>
  Task PublishStockResponseAsync(StockResponseMessage response);

  /// <summary>
  /// Subscribe to stock responses for a specific room
  /// </summary>
  /// <param name="roomId">The room ID to subscribe to</param>
  /// <param name="handler">Callback when stock response is received</param>
  void SubscribeToStockResponses(string roomId, Func<StockResponseMessage, Task> handler);

  /// <summary>
  /// Unsubscribe from stock responses for a specific room
  /// </summary>
  /// <param name="roomId">The room ID to unsubscribe from</param>
  void UnsubscribeFromStockResponses(string roomId);
}
