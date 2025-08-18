using System.Threading.Channels;
using System.Collections.Concurrent;
using ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Services
{
  public class InMemoryMessageBrokerService : IMessageBrokerService
  {
    private readonly Channel<StockRequestMessage> _stockRequestChannel;
    private readonly ConcurrentDictionary<string, List<Func<StockResponseMessage, Task>>> _stockResponseHandlers;
    private readonly ILogger<InMemoryMessageBrokerService> _logger;
    private readonly ISignalRNotificationService _signalRService;

    public InMemoryMessageBrokerService(
      ILogger<InMemoryMessageBrokerService> logger,
      ISignalRNotificationService signalRService)
    {
      _logger = logger;
      _signalRService = signalRService;
      
      // Create unbounded channel for stock requests
      var options = new UnboundedChannelOptions
      {
        SingleReader = true, // Only one background service will consume
        SingleWriter = false // Multiple SignalR connections can publish
      };
      
      _stockRequestChannel = Channel.CreateUnbounded<StockRequestMessage>(options);
      _stockResponseHandlers = new ConcurrentDictionary<string, List<Func<StockResponseMessage, Task>>>();
    }

    public async Task PublishStockRequestAsync(StockRequestMessage request)
    {
      _logger.LogInformation("Publishing stock request for symbol: {Symbol} in room: {RoomId}", 
        request.StockSymbol, request.RoomId);
      
      await _stockRequestChannel.Writer.WriteAsync(request);
    }

    public async Task<StockRequestMessage> ConsumeStockRequestAsync(CancellationToken cancellationToken = default)
    {
      return await _stockRequestChannel.Reader.ReadAsync(cancellationToken);
    }

    public async Task PublishStockResponseAsync(StockResponseMessage response)
    {
      _logger.LogInformation("Publishing stock response for room: {RoomId}", response.RoomId);
      
      // First, send via SignalR using the notification service
      try
      {
        await _signalRService.SendStockResponseToRoomAsync(response);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send stock response via SignalR for room: {RoomId}", response.RoomId);
      }

      // Then, call any registered handlers for backward compatibility
      if (_stockResponseHandlers.TryGetValue(response.RoomId, out var handlers))
      {
        var tasks = handlers.Select(handler => 
        {
          try
          {
            return handler(response);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error in stock response handler for room: {RoomId}", response.RoomId);
            return Task.CompletedTask;
          }
        }).ToArray();
        
        try
        {
          await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error executing stock response handlers for room: {RoomId}", response.RoomId);
        }
      }
    }

    public void SubscribeToStockResponses(string roomId, Func<StockResponseMessage, Task> handler)
    {
      _logger.LogInformation("Subscribing to stock responses for room: {RoomId}", roomId);
      
      _stockResponseHandlers.AddOrUpdate(roomId,
        new List<Func<StockResponseMessage, Task>> { handler },
        (key, existing) =>
        {
          existing.Add(handler);
          return existing;
        });
    }

    public void UnsubscribeFromStockResponses(string roomId)
    {
      _logger.LogInformation("Unsubscribing from stock responses for room: {RoomId}", roomId);
      _stockResponseHandlers.TryRemove(roomId, out _);
    }
  }
}
