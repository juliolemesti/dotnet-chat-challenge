using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using ChatChallenge.Api.Services;
using ChatChallenge.Api.Models;
using System.Threading.Channels;

namespace ChatChallenge.Tests
{
  public class StockBotBackgroundServiceTests
  {
    private readonly Mock<IMessageBrokerService> _messageBrokerMock;
    private readonly Mock<IStockApiService> _stockApiServiceMock;
    private readonly Mock<ILogger<StockBotBackgroundService>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly StockBotBackgroundService _backgroundService;

    public StockBotBackgroundServiceTests()
    {
      _messageBrokerMock = new Mock<IMessageBrokerService>();
      _stockApiServiceMock = new Mock<IStockApiService>();
      _loggerMock = new Mock<ILogger<StockBotBackgroundService>>();
      _serviceProviderMock = new Mock<IServiceProvider>();

      _backgroundService = new StockBotBackgroundService(
        _messageBrokerMock.Object,
        _stockApiServiceMock.Object,
        _loggerMock.Object,
        _serviceProviderMock.Object
      );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessStockRequest_WhenRequestReceived()
    {
      // Arrange
      var stockRequest = new StockRequestMessage
      {
        StockSymbol = "AAPL.US",
        RoomId = "room123",
        RequestedBy = "testuser",
        RequestedAt = DateTime.UtcNow
      };

      var stockResult = new StockQuoteResult
      {
        IsSuccess = true,
        StockSymbol = "AAPL.US",
        Price = 150.00m,
        FormattedMessage = "AAPL.US quote is $150.00 per share"
      };

      var cancellationTokenSource = new CancellationTokenSource();
      
      // Setup the message broker to return our request once, then cancel
      _messageBrokerMock
        .SetupSequence(x => x.ConsumeStockRequestAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(stockRequest)
        .Throws(new OperationCanceledException()); // This will stop the background service

      _stockApiServiceMock
        .Setup(x => x.GetStockQuoteAsync(stockRequest.StockSymbol))
        .ReturnsAsync(stockResult);

      // Act
      await _backgroundService.StartAsync(cancellationTokenSource.Token);
      
      // Give some time for processing
      await Task.Delay(100);
      
      cancellationTokenSource.Cancel();
      await _backgroundService.StopAsync(CancellationToken.None);

      // Assert
      _messageBrokerMock.Verify(
        x => x.ConsumeStockRequestAsync(It.IsAny<CancellationToken>()),
        Times.AtLeastOnce);

      _stockApiServiceMock.Verify(
        x => x.GetStockQuoteAsync(stockRequest.StockSymbol),
        Times.Once);

      _messageBrokerMock.Verify(
        x => x.PublishStockResponseAsync(It.Is<StockResponseMessage>(r => 
          r.StockSymbol == stockRequest.StockSymbol &&
          r.RoomId == stockRequest.RoomId &&
          r.RequestedBy == stockRequest.RequestedBy &&
          !r.IsError &&
          r.FormattedMessage == stockResult.FormattedMessage)),
        Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleStockApiError_WhenApiCallFails()
    {
      // Arrange
      var stockRequest = new StockRequestMessage
      {
        StockSymbol = "INVALID",
        RoomId = "room456",
        RequestedBy = "testuser2",
        RequestedAt = DateTime.UtcNow
      };

      var stockResult = new StockQuoteResult
      {
        IsSuccess = false,
        StockSymbol = "INVALID",
        ErrorMessage = "Stock not found"
      };

      var cancellationTokenSource = new CancellationTokenSource();

      _messageBrokerMock
        .SetupSequence(x => x.ConsumeStockRequestAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(stockRequest)
        .Throws(new OperationCanceledException());

      _stockApiServiceMock
        .Setup(x => x.GetStockQuoteAsync(stockRequest.StockSymbol))
        .ReturnsAsync(stockResult);

      // Act
      await _backgroundService.StartAsync(cancellationTokenSource.Token);
      await Task.Delay(100);
      cancellationTokenSource.Cancel();
      await _backgroundService.StopAsync(CancellationToken.None);

      // Assert
      _messageBrokerMock.Verify(
        x => x.PublishStockResponseAsync(It.Is<StockResponseMessage>(r => 
          r.StockSymbol == stockRequest.StockSymbol &&
          r.IsError &&
          r.ErrorMessage == stockResult.ErrorMessage)),
        Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleException_WhenStockApiServiceThrows()
    {
      // Arrange
      var stockRequest = new StockRequestMessage
      {
        StockSymbol = "TEST",
        RoomId = "room789",
        RequestedBy = "testuser3",
        RequestedAt = DateTime.UtcNow
      };

      var cancellationTokenSource = new CancellationTokenSource();

      _messageBrokerMock
        .SetupSequence(x => x.ConsumeStockRequestAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(stockRequest)
        .Throws(new OperationCanceledException());

      _stockApiServiceMock
        .Setup(x => x.GetStockQuoteAsync(It.IsAny<string>()))
        .ThrowsAsync(new HttpRequestException("Network error"));

      // Act
      await _backgroundService.StartAsync(cancellationTokenSource.Token);
      await Task.Delay(100);
      cancellationTokenSource.Cancel();
      await _backgroundService.StopAsync(CancellationToken.None);

      // Assert - should publish error response
      _messageBrokerMock.Verify(
        x => x.PublishStockResponseAsync(It.Is<StockResponseMessage>(r => 
          r.StockSymbol == stockRequest.StockSymbol &&
          r.IsError &&
          r.ErrorMessage == "Internal server error while processing stock request")),
        Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinueProcessing_WhenSingleRequestFails()
    {
      // Arrange
      var stockRequest1 = new StockRequestMessage { StockSymbol = "AAPL.US", RoomId = "room1", RequestedBy = "user1" };
      var stockRequest2 = new StockRequestMessage { StockSymbol = "MSFT.US", RoomId = "room2", RequestedBy = "user2" };
      
      var stockResult2 = new StockQuoteResult
      {
        IsSuccess = true,
        StockSymbol = "MSFT.US",
        FormattedMessage = "MSFT.US quote is $200.00 per share"
      };

      var cancellationTokenSource = new CancellationTokenSource();

      _messageBrokerMock
        .SetupSequence(x => x.ConsumeStockRequestAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(stockRequest1)
        .ReturnsAsync(stockRequest2)
        .Throws(new OperationCanceledException());

      // First request throws exception, second succeeds
      _stockApiServiceMock
        .Setup(x => x.GetStockQuoteAsync("AAPL.US"))
        .ThrowsAsync(new Exception("API Error"));

      _stockApiServiceMock
        .Setup(x => x.GetStockQuoteAsync("MSFT.US"))
        .ReturnsAsync(stockResult2);

      // Act
      await _backgroundService.StartAsync(cancellationTokenSource.Token);
      await Task.Delay(200); // Give more time for two requests
      cancellationTokenSource.Cancel();
      await _backgroundService.StopAsync(CancellationToken.None);

      // Assert - should have processed both requests
      _messageBrokerMock.Verify(
        x => x.ConsumeStockRequestAsync(It.IsAny<CancellationToken>()),
        Times.AtLeast(2));

      // Should publish error response for first request
      _messageBrokerMock.Verify(
        x => x.PublishStockResponseAsync(It.Is<StockResponseMessage>(r => 
          r.StockSymbol == "AAPL.US" && r.IsError)),
        Times.Once);

      // Should publish success response for second request
      _messageBrokerMock.Verify(
        x => x.PublishStockResponseAsync(It.Is<StockResponseMessage>(r => 
          r.StockSymbol == "MSFT.US" && !r.IsError)),
        Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldLogStoppedMessage()
    {
      // Act
      await _backgroundService.StopAsync(CancellationToken.None);

      // Assert
      _loggerMock.Verify(
        x => x.Log(
          LogLevel.Information,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Stock Bot Background Service is stopping")),
          It.IsAny<Exception?>(),
          It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }
  }
}
