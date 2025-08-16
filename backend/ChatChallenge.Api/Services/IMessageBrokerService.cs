using ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Services
{
  public interface IMessageBrokerService
  {
    Task PublishStockRequestAsync(StockRequestMessage request);
    Task<StockRequestMessage> ConsumeStockRequestAsync(CancellationToken cancellationToken = default);
    Task PublishStockResponseAsync(StockResponseMessage response);
    void SubscribeToStockResponses(string roomId, Func<StockResponseMessage, Task> handler);
    void UnsubscribeFromStockResponses(string roomId);
  }
}
