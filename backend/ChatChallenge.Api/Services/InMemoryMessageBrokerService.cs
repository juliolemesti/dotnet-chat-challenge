using System.Threading.Channels;
using System.Collections.Concurrent;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.DTOs;

namespace ChatChallenge.Api.Services
{
  public class InMemoryMessageBrokerService : IMessageBrokerService
  {
    private readonly Channel<StockRequestMessage> _stockRequestChannel;
    private readonly ConcurrentDictionary<string, List<Func<StockResponseMessage, Task>>> _stockResponseHandlers;
    private readonly ILogger<InMemoryMessageBrokerService> _logger;

    public InMemoryMessageBrokerService(
      ILogger<InMemoryMessageBrokerService> logger)
    {
      _logger = logger;
      
      var options = new UnboundedChannelOptions
      {
        SingleReader = true, 
        SingleWriter = false
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
      _logger.LogInformation("📢 Publishing stock response for room: {RoomId}, Symbol: {Symbol}", 
        response.RoomId, response.StockSymbol);
      _logger.LogInformation("📢 Stock response message: {Message}", response.FormattedMessage);

      if (_stockResponseHandlers.TryGetValue(response.RoomId, out var handlers))
      {
        _logger.LogInformation("✅ Found {HandlerCount} handlers for room {RoomId}", handlers.Count, response.RoomId);
        
        var tasks = handlers.Select(handler => 
        {
          try
          {
            _logger.LogInformation("🚀 Invoking handler for room {RoomId}", response.RoomId);
            return handler(response);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "❌ Error in stock response handler for room: {RoomId}", response.RoomId);
            return Task.CompletedTask;
          }
        }).ToArray();
        
        try
        {
          await Task.WhenAll(tasks);
          _logger.LogInformation("✅ All handlers completed for room {RoomId}", response.RoomId);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "❌ Error executing stock response handlers for room: {RoomId}", response.RoomId);
        }
      }
      else
      {
        _logger.LogWarning("⚠️ No handlers found for room {RoomId} - stock response will be lost!", response.RoomId);
        _logger.LogWarning("⚠️ Available rooms with handlers: {AvailableRooms}", 
          string.Join(", ", _stockResponseHandlers.Keys));
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
