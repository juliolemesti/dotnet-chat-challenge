using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Diagnostics;
using ChatChallenge.Api.Services;
using ChatChallenge.Api.Models;
using ChatChallenge.Core.Interfaces;

namespace ChatChallenge.Tests;

/// <summary>
/// End-to-end integration tests for the complete stock bot system
/// Tests the full workflow from request to response
/// </summary>
public class StockBotEndToEndIntegrationTests : IDisposable
{
  private readonly ServiceProvider _serviceProvider;
  private readonly IMessageBrokerService _messageBroker;
  private readonly IStockBotService _stockBotService;
  private readonly IStockApiService _stockApiService;
  private readonly StockBotBackgroundService _backgroundService;

  public StockBotEndToEndIntegrationTests()
  {
    var services = new ServiceCollection();
    
    services.AddLogging();
    services.AddHttpClient<IStockApiService, StockApiService>();
    
    // Mock SignalR notification service for testing
    var mockSignalRService = new Mock<ISignalRNotificationService>();
    services.AddScoped(_ => mockSignalRService.Object);
    
    services.AddSingleton<IMessageBrokerService, InMemoryMessageBrokerService>();
    services.AddScoped<IStockBotService, StockBotService>();
    services.AddHostedService<StockBotBackgroundService>();
    
    var mockChatRepository = new Mock<IChatRepository>();
    services.AddScoped(_ => mockChatRepository.Object);
    
    _serviceProvider = services.BuildServiceProvider();
    _messageBroker = _serviceProvider.GetRequiredService<IMessageBrokerService>();
    _stockBotService = _serviceProvider.GetRequiredService<IStockBotService>();
    _stockApiService = _serviceProvider.GetRequiredService<IStockApiService>();
    _backgroundService = _serviceProvider.GetServices<IHostedService>()
      .OfType<StockBotBackgroundService>()
      .First();
  }

  [Fact]
  public async Task CompleteWorkflow_ValidStock_ShouldProcessSuccessfully()
  {
    // Arrange
    var roomId = "test-room-1";
    var userId = "test-user-1";
    var stockSymbol = "AAPL.US";
    var responseReceived = false;
    StockResponseMessage? receivedResponse = null;

    _messageBroker.SubscribeToStockResponses(roomId, (response) =>
    {
      responseReceived = true;
      receivedResponse = response;
      return Task.CompletedTask;
    });

    var cancellationTokenSource = new CancellationTokenSource();
    var backgroundTask = _backgroundService.StartAsync(cancellationTokenSource.Token);

    try
    {
      // Act
      await _stockBotService.QueueStockRequestAsync(stockSymbol, userId, roomId);
      
      // Wait for processing with timeout
      var timeout = TimeSpan.FromSeconds(30);
      var stopwatch = Stopwatch.StartNew();
      
      while (!responseReceived && stopwatch.Elapsed < timeout)
      {
        await Task.Delay(100);
      }

      // Assert
      Assert.True(responseReceived, "Should receive stock response within 30 seconds");
      Assert.NotNull(receivedResponse);
      Assert.Equal(roomId, receivedResponse.RoomId);
      Assert.Equal(userId, receivedResponse.RequestedBy);
      Assert.Equal(stockSymbol, receivedResponse.StockSymbol);
      Assert.NotNull(receivedResponse.FormattedMessage);
      Assert.Contains("AAPL.US", receivedResponse.FormattedMessage);
    }
    finally
    {
      cancellationTokenSource.Cancel();
      try
      {
        await backgroundTask;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }
  }

  [Fact]
  public async Task CompleteWorkflow_MultipleRoomsSimultaneously_ShouldIsolateCorrectly()
  {
    // Arrange
    const int roomCount = 3;
    var responses = new Dictionary<string, StockResponseMessage?>();
    var responseFlags = new Dictionary<string, bool>();

    for (int i = 0; i < roomCount; i++)
    {
      var roomId = $"room-{i}";
      responses[roomId] = null;
      responseFlags[roomId] = false;

      _messageBroker.SubscribeToStockResponses(roomId, (response) =>
      {
        responses[response.RoomId] = response;
        responseFlags[response.RoomId] = true;
        return Task.CompletedTask;
      });
    }

    var cancellationTokenSource = new CancellationTokenSource();
    var backgroundTask = _backgroundService.StartAsync(cancellationTokenSource.Token);

    try
    {
      // Act - Send requests to all rooms simultaneously
      var tasks = new List<Task>();
      for (int i = 0; i < roomCount; i++)
      {
        var roomId = $"room-{i}";
        var userId = $"user-{i}";
        tasks.Add(_stockBotService.QueueStockRequestAsync("AAPL.US", userId, roomId));
      }
      
      await Task.WhenAll(tasks);

      // Wait for all responses with timeout
      var timeout = TimeSpan.FromSeconds(45);
      var stopwatch = Stopwatch.StartNew();
      
      while (responseFlags.Values.Any(f => !f) && stopwatch.Elapsed < timeout)
      {
        await Task.Delay(100);
      }

      // Assert - All rooms should receive their responses
      foreach (var roomId in responseFlags.Keys)
      {
        Assert.True(responseFlags[roomId], $"Room {roomId} should receive a response");
        Assert.NotNull(responses[roomId]);
        Assert.Equal(roomId, responses[roomId]!.RoomId);
        Assert.Equal($"user-{roomId.Split('-')[1]}", responses[roomId]!.RequestedBy);
      }
    }
    finally
    {
      cancellationTokenSource.Cancel();
      try
      {
        await backgroundTask;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }
  }

  [Fact]
  public async Task CompleteWorkflow_InvalidStock_ShouldReturnErrorMessage()
  {
    // Arrange
    var roomId = "error-test-room";
    var userId = "error-test-user";
    var invalidSymbol = "INVALID.STOCK";
    var responseReceived = false;
    StockResponseMessage? receivedResponse = null;

    _messageBroker.SubscribeToStockResponses(roomId, (response) =>
    {
      responseReceived = true;
      receivedResponse = response;
      return Task.CompletedTask;
    });

    var cancellationTokenSource = new CancellationTokenSource();
    var backgroundTask = _backgroundService.StartAsync(cancellationTokenSource.Token);

    try
    {
      // Act
      await _stockBotService.QueueStockRequestAsync(invalidSymbol, userId, roomId);
      
      // Wait for processing with timeout
      var timeout = TimeSpan.FromSeconds(30);
      var stopwatch = Stopwatch.StartNew();
      
      while (!responseReceived && stopwatch.Elapsed < timeout)
      {
        await Task.Delay(100);
      }

      // Assert
      Assert.True(responseReceived, "Should receive error response within 30 seconds");
      Assert.NotNull(receivedResponse);
      Assert.Equal(roomId, receivedResponse.RoomId);
      Assert.Equal(userId, receivedResponse.RequestedBy);
      Assert.Equal(invalidSymbol, receivedResponse.StockSymbol);
      Assert.NotNull(receivedResponse.FormattedMessage);
      
      // Should contain error indication
      Assert.True(receivedResponse.FormattedMessage.Contains("not available") || 
                  receivedResponse.FormattedMessage.Contains("error"),
                  "Response should indicate an error occurred");
    }
    finally
    {
      cancellationTokenSource.Cancel();
      try
      {
        await backgroundTask;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }
  }

  [Fact]
  public void StockBotService_CommandValidation_ShouldValidateCorrectly()
  {
    // Test various command formats
    var testCases = new[]
    {
      new { Command = "/stock=", Expected = "", Valid = false },
      new { Command = "/stock=INVALID@SYMBOL", Expected = "", Valid = false },
      new { Command = "/stock=", Expected = "", Valid = false },
      new { Command = "not a stock command", Expected = "", Valid = false },
      new { Command = "/stock=AAPL.US", Expected = "AAPL.US", Valid = true }
    };

    foreach (var testCase in testCases)
    {
      // Act
      var extractedSymbol = _stockBotService.ExtractStockSymbol(testCase.Command);
      var isValid = !string.IsNullOrEmpty(extractedSymbol) && 
                    _stockBotService.IsValidStockSymbol(extractedSymbol);

      // Assert
      if (testCase.Valid)
      {
        Assert.True(isValid, $"Command '{testCase.Command}' should be valid but was invalid");
        Assert.Equal(testCase.Expected, extractedSymbol);
      }
      else
      {
        Assert.False(isValid, $"Command '{testCase.Command}' should be invalid but was valid");
      }
    }
  }

  [Fact]
  public async Task MessageBroker_ConcurrentRequests_ShouldHandleGracefully()
  {
    // Arrange
    const int requestCount = 10;
    var roomId = "concurrent-test-room";
    var responseCount = 0;
    var allResponses = new List<StockResponseMessage>();

    _messageBroker.SubscribeToStockResponses(roomId, (response) =>
    {
      lock (allResponses)
      {
        allResponses.Add(response);
        Interlocked.Increment(ref responseCount);
      }
      return Task.CompletedTask;
    });

    var cancellationTokenSource = new CancellationTokenSource();
    var backgroundTask = _backgroundService.StartAsync(cancellationTokenSource.Token);

    try
    {
      // Act - Send multiple concurrent requests
      var tasks = Enumerable.Range(0, requestCount)
        .Select(i => _stockBotService.QueueStockRequestAsync("AAPL.US", $"user-{i}", roomId))
        .ToArray();
      
      await Task.WhenAll(tasks);

      // Wait for all responses with timeout
      var timeout = TimeSpan.FromMinutes(2);
      var stopwatch = Stopwatch.StartNew();
      
      while (responseCount < requestCount && stopwatch.Elapsed < timeout)
      {
        await Task.Delay(200);
      }

      // Assert
      Assert.Equal(requestCount, responseCount);
      Assert.Equal(requestCount, allResponses.Count);
      
      // Verify all responses are for the correct room and have different users
      Assert.All(allResponses, response => Assert.Equal(roomId, response.RoomId));
      var uniqueUsers = allResponses.Select(r => r.RequestedBy).Distinct().Count();
      Assert.Equal(requestCount, uniqueUsers);
    }
    finally
    {
      cancellationTokenSource.Cancel();
      try
      {
        await backgroundTask;
      }
      catch (OperationCanceledException)
      {
        // Expected
      }
    }
  }

  public void Dispose()
  {
    _serviceProvider?.Dispose();
  }
}
