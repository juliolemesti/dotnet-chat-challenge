using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ChatChallenge.Api.Services;
using ChatChallenge.Api.Models;

namespace ChatChallenge.Tests;

public class StockBotSignalRIntegrationTests
{
  private readonly Mock<ILogger<StockBotService>> _mockLogger;
  private readonly IMessageBrokerService _messageBrokerService;
  private readonly IStockBotService _stockBotService;

  public StockBotSignalRIntegrationTests()
  {
    _mockLogger = new Mock<ILogger<StockBotService>>();
    _messageBrokerService = new InMemoryMessageBrokerService(
      Mock.Of<ILogger<InMemoryMessageBrokerService>>());
    _stockBotService = new StockBotService(_messageBrokerService, _mockLogger.Object);
  }

  [Fact]
  public async Task QueueStockRequestAsync_ShouldPublishToMessageBroker()
  {
    // Arrange
    var stockSymbol = "AAPL.US";
    var requestedBy = "testuser";
    var roomId = "123";

    // Act
    await _stockBotService.QueueStockRequestAsync(stockSymbol, requestedBy, roomId);

    // Allow a brief moment for message processing
    await Task.Delay(10);

    // Consume the request
    var capturedRequest = await _messageBrokerService.ConsumeStockRequestAsync();

    // Assert
    Assert.NotNull(capturedRequest);
    Assert.Equal(stockSymbol, capturedRequest.StockSymbol);
    Assert.Equal(requestedBy, capturedRequest.RequestedBy);
    Assert.Equal(roomId, capturedRequest.RoomId);
    Assert.True(capturedRequest.RequestedAt <= DateTime.UtcNow);
  }

  [Fact]
  public void ExtractStockSymbol_ValidStockCommand_ReturnsCorrectSymbol()
  {
    // Arrange
    var command = "/stock=MSFT.US";

    // Act
    var result = _stockBotService.ExtractStockSymbol(command);

    // Assert
    Assert.Equal("MSFT.US", result);
  }

  [Fact]
  public void ExtractStockSymbol_InvalidStockCommand_ReturnsNull()
  {
    // Arrange
    var command = "not a stock command";

    // Act
    var result = _stockBotService.ExtractStockSymbol(command);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public void IsValidStockSymbol_ValidSymbol_ReturnsTrue()
  {
    // Arrange & Act & Assert
    Assert.True(_stockBotService.IsValidStockSymbol("AAPL.US"));
    Assert.True(_stockBotService.IsValidStockSymbol("GOOGL"));
    Assert.True(_stockBotService.IsValidStockSymbol("BRK-B"));
  }

  [Fact]
  public void IsValidStockSymbol_InvalidSymbol_ReturnsFalse()
  {
    // Arrange & Act & Assert
    Assert.False(_stockBotService.IsValidStockSymbol(""));
    Assert.False(_stockBotService.IsValidStockSymbol("INVALID@SYMBOL"));
    Assert.False(_stockBotService.IsValidStockSymbol("VERYLONGSTOCKSYMBOL123456"));
  }

  [Fact]
  public async Task SignalRIntegration_StockResponseSubscription_ShouldReceiveResponses()
  {
    // Arrange
    var roomId = "456";
    StockResponseMessage? capturedResponse = null;

    _messageBrokerService.SubscribeToStockResponses(roomId, (response) =>
    {
      capturedResponse = response;
      return Task.CompletedTask;
    });

    var stockResponse = new StockResponseMessage
    {
      StockSymbol = "TSLA.US",
      RoomId = roomId,
      FormattedMessage = "TSLA.US quote is $250.45 per share",
      RequestedBy = "testuser",
      ResponseAt = DateTime.UtcNow,
      IsError = false
    };

    // Act
    await _messageBrokerService.PublishStockResponseAsync(stockResponse);

    // Allow a brief moment for message processing
    await Task.Delay(10);

    // Assert
    Assert.NotNull(capturedResponse);
    Assert.Equal("TSLA.US", capturedResponse.StockSymbol);
    Assert.Equal(roomId, capturedResponse.RoomId);
    Assert.Equal("TSLA.US quote is $250.45 per share", capturedResponse.FormattedMessage);
    Assert.False(capturedResponse.IsError);
  }
}
