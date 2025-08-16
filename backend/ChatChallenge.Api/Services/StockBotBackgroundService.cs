using Microsoft.Extensions.Hosting;
using ChatChallenge.Api.Services;
using ChatChallenge.Api.Models;

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
            // Wait for stock requests from the message broker
            var request = await _messageBrokerService.ConsumeStockRequestAsync(stoppingToken);
            
            _logger.LogInformation("Processing stock request for symbol: {Symbol} from user: {User} in room: {RoomId}",
              request.StockSymbol, request.RequestedBy, request.RoomId);

            // Process the stock request
            _ = Task.Run(async () => await ProcessStockRequestAsync(request), stoppingToken);
          }
          catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
          {
            // Expected when service is stopping
            break;
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error consuming stock request from message broker");
            
            // Brief delay before retrying to avoid tight loop on persistent errors
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
      try
      {
        // Call the Stock API service to get the quote
        var result = await _stockApiService.GetStockQuoteAsync(request.StockSymbol);

        var response = new StockResponseMessage
        {
          StockSymbol = request.StockSymbol,
          RoomId = request.RoomId,
          RequestedBy = request.RequestedBy,
          ResponseAt = DateTime.UtcNow,
          IsError = !result.IsSuccess,
          ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
          FormattedMessage = result.IsSuccess ? result.FormattedMessage : $"Error getting stock quote for {request.StockSymbol}: {result.ErrorMessage}"
        };

        // Publish the response back through the message broker
        await _messageBrokerService.PublishStockResponseAsync(response);

        _logger.LogInformation("Successfully processed stock request for {Symbol}: {Message}",
          request.StockSymbol, response.FormattedMessage);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error processing stock request for symbol: {Symbol}", request.StockSymbol);

        // Send error response
        var errorResponse = new StockResponseMessage
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
          await _messageBrokerService.PublishStockResponseAsync(errorResponse);
        }
        catch (Exception publishEx)
        {
          _logger.LogError(publishEx, "Failed to publish error response for stock request");
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
