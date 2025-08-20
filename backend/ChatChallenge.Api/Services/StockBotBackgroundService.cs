using Microsoft.Extensions.Hosting;
using ChatChallenge.Api.Services;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.DTOs;

namespace ChatChallenge.Api.Services
{
  public class StockBotBackgroundService : BackgroundService
  {
    private readonly IMessageBrokerService _messageBrokerService;
    private readonly IStockApiService _stockApiService;
    private readonly ILogger<StockBotBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public StockBotBackgroundService(
      IMessageBrokerService messageBrokerService,
      IStockApiService stockApiService,
      ILogger<StockBotBackgroundService> logger,
      IServiceProvider serviceProvider)
    {
      _messageBrokerService = messageBrokerService;
      _stockApiService = stockApiService;
      _logger = logger;
      _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Stock Bot Background Service started");

      try
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          try
          {
            var request = await _messageBrokerService.ConsumeStockRequestAsync(stoppingToken);
            
            _logger.LogInformation("Processing stock request for symbol: {Symbol} from user: {User} in room: {RoomId}",
              request.StockSymbol, request.RequestedBy, request.RoomId);

            _ = Task.Run(async () => await ProcessStockRequestAsync(request), stoppingToken);
          }
          catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
          {
            break;
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error consuming stock request from message broker");
            
            await Task.Delay(1000, stoppingToken);
          }
        }
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        // Expected when service is stopping
      }
      catch (Exception ex)
      {
        _logger.LogCritical(ex, "Critical error in Stock Bot Background Service");
      }
      finally
      {
        _logger.LogInformation("Stock Bot Background Service stopped");
      }
    }

    private async Task ProcessStockRequestAsync(StockRequestMessage request)
    {
      using var scope = _serviceProvider.CreateScope();
      var signalRNotificationService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
      
      try
      {
        _logger.LogInformation("🔍 Calling Stock API for symbol: {Symbol}", request.StockSymbol);
        
        var result = await _stockApiService.GetStockQuoteAsync(request.StockSymbol);

        _logger.LogInformation("📊 Stock API result for {Symbol}: Success={IsSuccess}, Message={Message}", 
          request.StockSymbol, result.IsSuccess, result.FormattedMessage);

        var stockResponse = new ChatChallenge.Application.DTOs.StockResponseMessage
        {
          StockSymbol = request.StockSymbol,
          RoomId = request.RoomId,
          RequestedBy = request.RequestedBy,
          ResponseAt = DateTime.UtcNow,
          IsError = !result.IsSuccess,
          ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
          FormattedMessage = result.IsSuccess ? result.FormattedMessage : $"Error getting stock quote for {request.StockSymbol}: {result.ErrorMessage}"
        };

        _logger.LogInformation("📤 Sending stock response directly to SignalR - Room: {RoomId}, Symbol: {Symbol}, Message: {Message}",
          stockResponse.RoomId, stockResponse.StockSymbol, stockResponse.FormattedMessage);

        await signalRNotificationService.SendStockResponseToRoomAsync(stockResponse);

        _logger.LogInformation("✅ Successfully processed and sent stock request for {Symbol}: {Message}",
          request.StockSymbol, stockResponse.FormattedMessage);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error processing stock request for symbol: {Symbol}", request.StockSymbol);

        var errorResponse = new ChatChallenge.Application.DTOs.StockResponseMessage
        {
          StockSymbol = request.StockSymbol,
          RoomId = request.RoomId,
          RequestedBy = request.RequestedBy,
          ResponseAt = DateTime.UtcNow,
          IsError = true,
          ErrorMessage = "Internal server error while processing stock request",
          FormattedMessage = $"Sorry, I couldn't get the stock quote for {request.StockSymbol} due to a technical error."
        };

        try
        {
          _logger.LogInformation("📤 Sending error response directly to SignalR - Room: {RoomId}", errorResponse.RoomId);
          await signalRNotificationService.SendStockResponseToRoomAsync(errorResponse);
        }
        catch (Exception signalREx)
        {
          _logger.LogError(signalREx, "Failed to send error response via SignalR for stock request");
        }
      }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("Stock Bot Background Service is stopping...");
      await base.StopAsync(cancellationToken);
    }
  }
}
