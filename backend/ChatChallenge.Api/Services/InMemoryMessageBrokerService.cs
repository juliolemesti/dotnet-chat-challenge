using System.Threading.Channels;
using System.Collections.Concurrent;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.DTOs;
using ApiModels = ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Services
{
  public class InMemoryMessageBrokerService : 
    ChatChallenge.Application.Interfaces.IMessageBrokerService,
    ChatChallenge.Api.Services.IMessageBrokerService
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

    // Implementation of Application.Interfaces.IMessageBrokerService
    public async Task PublishStockRequestAsync(StockRequestMessage request)
    {
      _logger.LogInformation("Publishing stock request for symbol: {Symbol} in room: {RoomId}", 
        request.StockSymbol, request.RoomId);
      
      await _stockRequestChannel.Writer.WriteAsync(request);
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

    // Implementation of Api.Services.IMessageBrokerService
    async Task ChatChallenge.Api.Services.IMessageBrokerService.PublishStockRequestAsync(ApiModels.StockRequestMessage request)
    {
      // Convert API model to Application model
      var appRequest = new StockRequestMessage
      {
        StockSymbol = request.StockSymbol,
        RoomId = request.RoomId,
        RequestedBy = request.RequestedBy,
        RequestedAt = request.RequestedAt
      };
      
      await PublishStockRequestAsync(appRequest);
    }

    public async Task<ApiModels.StockRequestMessage> ConsumeStockRequestAsync(CancellationToken cancellationToken = default)
    {
      var appRequest = await _stockRequestChannel.Reader.ReadAsync(cancellationToken);
      
      // Convert Application model to API model
      return new ApiModels.StockRequestMessage
      {
        StockSymbol = appRequest.StockSymbol,
        RoomId = appRequest.RoomId,
        RequestedBy = appRequest.RequestedBy,
        RequestedAt = appRequest.RequestedAt
      };
    }

    async Task ChatChallenge.Api.Services.IMessageBrokerService.PublishStockResponseAsync(ApiModels.StockResponseMessage response)
    {
      // Convert API model to Application model
      var appResponse = new StockResponseMessage
      {
        StockSymbol = response.StockSymbol,
        RoomId = response.RoomId,
        RequestedBy = response.RequestedBy,
        ResponseAt = response.ResponseAt,
        IsError = response.IsError,
        ErrorMessage = response.ErrorMessage,
        FormattedMessage = response.FormattedMessage
      };

      await PublishStockResponseInternalAsync(appResponse);
    }

    void ChatChallenge.Api.Services.IMessageBrokerService.SubscribeToStockResponses(string roomId, Func<ApiModels.StockResponseMessage, Task> handler)
    {
      // Wrap the API handler to convert between models
      Func<StockResponseMessage, Task> wrappedHandler = async (appResponse) =>
      {
        var apiResponse = new ApiModels.StockResponseMessage
        {
          StockSymbol = appResponse.StockSymbol,
          RoomId = appResponse.RoomId,
          RequestedBy = appResponse.RequestedBy,
          ResponseAt = appResponse.ResponseAt,
          IsError = appResponse.IsError,
          ErrorMessage = appResponse.ErrorMessage,
          FormattedMessage = appResponse.FormattedMessage
        };
        
        await handler(apiResponse);
      };

      SubscribeToStockResponses(roomId, wrappedHandler);
    }

    void ChatChallenge.Api.Services.IMessageBrokerService.UnsubscribeFromStockResponses(string roomId)
    {
      UnsubscribeFromStockResponses(roomId);
    }

    // Internal method for publishing Application responses
    private async Task PublishStockResponseInternalAsync(StockResponseMessage response)
    {
      _logger.LogInformation("Publishing stock response for room: {RoomId}", response.RoomId);
      _logger.LogInformation("Publishing stock response for room: {RoomId}, Symbol: {Symbol}", 
        response.RoomId, response.StockSymbol);

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
  }
}
