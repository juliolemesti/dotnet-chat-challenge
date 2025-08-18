using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using ChatChallenge.Api.Services;
using ChatChallenge.Api.Models;

namespace ChatChallenge.Tests
{
  public class InMemoryMessageBrokerServiceTests
  {
    private readonly Mock<ILogger<InMemoryMessageBrokerService>> _loggerMock;
    private readonly Mock<ISignalRNotificationService> _signalRServiceMock;
    private readonly InMemoryMessageBrokerService _messageBrokerService;

    public InMemoryMessageBrokerServiceTests()
    {
      _loggerMock = new Mock<ILogger<InMemoryMessageBrokerService>>();
      _signalRServiceMock = new Mock<ISignalRNotificationService>();
      _messageBrokerService = new InMemoryMessageBrokerService(_loggerMock.Object, _signalRServiceMock.Object);
    }

    [Fact]
    public async Task PublishAndConsumeStockRequest_ShouldWorkCorrectly()
    {
      // Arrange
      var request = new StockRequestMessage
      {
        StockSymbol = "AAPL.US",
        RoomId = "room123",
        RequestedBy = "testuser",
        RequestedAt = DateTime.UtcNow
      };

      // Act
      await _messageBrokerService.PublishStockRequestAsync(request);
      var consumedRequest = await _messageBrokerService.ConsumeStockRequestAsync();

      // Assert
      Assert.Equal(request.StockSymbol, consumedRequest.StockSymbol);
      Assert.Equal(request.RoomId, consumedRequest.RoomId);
      Assert.Equal(request.RequestedBy, consumedRequest.RequestedBy);
    }

    [Fact]
    public async Task PublishStockResponse_WithSubscribedHandler_ShouldCallHandler()
    {
      // Arrange
      var response = new StockResponseMessage
      {
        StockSymbol = "AAPL.US",
        RoomId = "room123",
        FormattedMessage = "AAPL.US quote is $150.00 per share",
        RequestedBy = "testuser"
      };

      var handlerCalled = false;
      StockResponseMessage? receivedResponse = null;

      Task handler(StockResponseMessage msg)
      {
        handlerCalled = true;
        receivedResponse = msg;
        return Task.CompletedTask;
      }

      // Act
      _messageBrokerService.SubscribeToStockResponses("room123", handler);
      await _messageBrokerService.PublishStockResponseAsync(response);

      // Give a small delay for async processing
      await Task.Delay(10);

      // Assert
      Assert.True(handlerCalled);
      Assert.NotNull(receivedResponse);
      Assert.Equal(response.StockSymbol, receivedResponse.StockSymbol);
      Assert.Equal(response.FormattedMessage, receivedResponse.FormattedMessage);
    }

    [Fact]
    public async Task PublishStockResponse_ShouldCallSignalRService()
    {
      // Arrange
      var response = new StockResponseMessage
      {
        StockSymbol = "AAPL.US",
        RoomId = "room456",
        FormattedMessage = "AAPL.US quote is $150.00 per share"
      };

      // Act
      await _messageBrokerService.PublishStockResponseAsync(response);

      // Assert - verify SignalR service was called
      _signalRServiceMock.Verify(
        x => x.SendStockResponseToRoomAsync(It.Is<StockResponseMessage>(r => 
          r.StockSymbol == response.StockSymbol && 
          r.RoomId == response.RoomId && 
          r.FormattedMessage == response.FormattedMessage)),
        Times.Once);
    }

    [Fact]
    public void UnsubscribeFromStockResponses_ShouldRemoveHandler()
    {
      // Arrange
      var handlerCallCount = 0;
      Task handler(StockResponseMessage msg)
      {
        handlerCallCount++;
        return Task.CompletedTask;
      }

      var response = new StockResponseMessage
      {
        RoomId = "room789",
        FormattedMessage = "Test message"
      };

      // Act
      _messageBrokerService.SubscribeToStockResponses("room789", handler);
      _messageBrokerService.UnsubscribeFromStockResponses("room789");
      
      // This should not call the handler since we unsubscribed
      _ = _messageBrokerService.PublishStockResponseAsync(response);

      // Assert
      Assert.Equal(0, handlerCallCount);
    }

    [Fact]
    public async Task MultipleHandlersForSameRoom_ShouldCallAllHandlers()
    {
      // Arrange
      var handler1Called = false;
      var handler2Called = false;
      
      Task handler1(StockResponseMessage msg)
      {
        handler1Called = true;
        return Task.CompletedTask;
      }
      
      Task handler2(StockResponseMessage msg)
      {
        handler2Called = true;
        return Task.CompletedTask;
      }

      var response = new StockResponseMessage
      {
        RoomId = "room999",
        FormattedMessage = "Test message"
      };

      // Act
      _messageBrokerService.SubscribeToStockResponses("room999", handler1);
      _messageBrokerService.SubscribeToStockResponses("room999", handler2);
      await _messageBrokerService.PublishStockResponseAsync(response);

      // Give a small delay for async processing
      await Task.Delay(10);

      // Assert
      Assert.True(handler1Called);
      Assert.True(handler2Called);
    }
  }
}
