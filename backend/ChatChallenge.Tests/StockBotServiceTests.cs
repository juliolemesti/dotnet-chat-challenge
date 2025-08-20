using Microsoft.Extensions.Logging;
using Moq;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.Services;
using ChatChallenge.Application.DTOs;

namespace ChatChallenge.Tests;

public class StockBotServiceSimpleTests
{
  private readonly Mock<IMessageBrokerService> _mockMessageBroker;
  private readonly Mock<ILogger<StockBotService>> _mockLogger;
  private readonly IStockBotService _stockBotService;

  public StockBotServiceSimpleTests()
  {
    _mockMessageBroker = new Mock<IMessageBrokerService>();
    _mockLogger = new Mock<ILogger<StockBotService>>();

    _stockBotService = new StockBotService(
      _mockMessageBroker.Object,
      _mockLogger.Object
    );
  }

  [Fact]
  public async Task QueueStockRequestAsync_ShouldCallMessageBroker()
  {
    // Arrange
    string stockSymbol = "AAPL.US";
    string requestedBy = "testuser";
    string roomId = "room1";

    // Act
    await _stockBotService.QueueStockRequestAsync(stockSymbol, requestedBy, roomId);

    // Assert
    _mockMessageBroker.Verify(
      m => m.PublishStockRequestAsync(It.Is<StockRequestMessage>(req =>
        req.StockSymbol == stockSymbol &&
        req.RequestedBy == requestedBy &&
        req.RoomId == roomId)),
      Times.Once
    );
  }

  [Fact]
  public async Task QueueStockRequestAsync_ShouldThrowException_WhenMessageBrokerFails()
  {
    // Arrange
    _mockMessageBroker.Setup(m => m.PublishStockRequestAsync(It.IsAny<StockRequestMessage>()))
      .ThrowsAsync(new Exception("Broker error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(
      () => _stockBotService.QueueStockRequestAsync("AAPL.US", "testuser", "room1")
    );
  }

  [Theory]
  [InlineData("AAPL.US", true)]
  [InlineData("MSFT", true)]
  [InlineData("GOOG", true)]
  [InlineData("AMZN.US", true)]
  [InlineData("BTC-USD", true)]
  [InlineData("", false)]
  [InlineData(" ", false)]
  [InlineData(null, false)]
  [InlineData("VERYLONGSTOCKSYMBOLNAME", false)]
  [InlineData("STOCK WITH SPACES", false)]
  [InlineData("STOCK@SYMBOL", false)]
  public void IsValidStockSymbol_ShouldValidateCorrectly(string? symbol, bool expected)
  {
    // Act
    var result = _stockBotService.IsValidStockSymbol(symbol!);

    // Assert
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("/stock=AAPL.US", "AAPL.US")]
  [InlineData("/stock=msft", "MSFT")]
  [InlineData("/STOCK=GOOG", "GOOG")]
  [InlineData("/stock= AMZN.US ", "AMZN.US")]
  [InlineData("/stock=", null)]
  [InlineData("/stock=INVALID@SYMBOL", null)]
  [InlineData("not a stock command", null)]
  [InlineData("/help", null)]
  [InlineData("", null)]
  [InlineData(null, null)]
  public void ExtractStockSymbol_ShouldExtractCorrectly(string? command, string? expected)
  {
    // Act
    var result = _stockBotService.ExtractStockSymbol(command!);

    // Assert
    Assert.Equal(expected, result);
  }
}
