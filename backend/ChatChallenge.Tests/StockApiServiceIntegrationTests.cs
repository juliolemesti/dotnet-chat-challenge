using ChatChallenge.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ChatChallenge.Tests;

/// <summary>
/// Integration tests for StockApiService (these call real APIs)
/// </summary>
public class StockApiServiceIntegrationTests
{
  [Fact]
  public async Task GetStockQuoteAsync_RealApi_ReturnsValidResult()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddHttpClient<IStockApiService, StockApiService>();
    services.AddLogging(builder => builder.AddConsole());
    
    var serviceProvider = services.BuildServiceProvider();
    var stockApiService = serviceProvider.GetRequiredService<IStockApiService>();

    // Act - Make a real call to AAPL.US (Apple stock)
    var result = await stockApiService.GetStockQuoteAsync("AAPL.US");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("AAPL.US", result.StockSymbol);
    
    if (result.IsSuccess)
    {
      // If successful, verify we have valid data
      Assert.NotNull(result.Price);
      Assert.True(result.Price > 0);
      Assert.Contains("AAPL.US quote is $", result.FormattedMessage);
      Assert.Contains("per share", result.FormattedMessage);
      Assert.Null(result.ErrorMessage);
    }
    else
    {
      // If failed, verify we have proper error handling
      Assert.Null(result.Price);
      Assert.NotNull(result.ErrorMessage);
      Assert.Contains("not available", result.FormattedMessage);
    }
  }

  [Fact]
  public async Task GetStockQuoteAsync_InvalidSymbol_ReturnsErrorResult()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddHttpClient<IStockApiService, StockApiService>();
    services.AddLogging(builder => builder.AddConsole());
    
    var serviceProvider = services.BuildServiceProvider();
    var stockApiService = serviceProvider.GetRequiredService<IStockApiService>();

    // Act - Try to get quote for invalid stock symbol
    var result = await stockApiService.GetStockQuoteAsync("INVALID_SYMBOL");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("INVALID_SYMBOL", result.StockSymbol);
    Assert.False(result.IsSuccess);
    Assert.Null(result.Price);
    Assert.NotNull(result.ErrorMessage);
    Assert.Contains("not available", result.FormattedMessage);
  }
}
