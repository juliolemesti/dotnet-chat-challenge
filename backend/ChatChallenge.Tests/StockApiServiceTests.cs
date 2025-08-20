using Microsoft.Extensions.Logging;
using ChatChallenge.Api.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace ChatChallenge.Tests;

public class StockApiServiceTests
{
  private readonly Mock<ILogger<StockApiService>> _mockLogger;
  private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
  private readonly HttpClient _httpClient;
  private readonly StockApiService _stockApiService;

  public StockApiServiceTests()
  {
    _mockLogger = new Mock<ILogger<StockApiService>>();
    _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    
    _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    _stockApiService = new StockApiService(_httpClient, _mockLogger.Object);
  }

  [Fact]
  public async Task GetStockQuoteAsync_ValidResponse_ReturnsSuccessResult()
  {
    var stockSymbol = "AAPL.US";
    var csvResponse = "Symbol,Date,Time,Open,High,Low,Close,Volume\nAAPL.US,2023-10-20,22:00:01,173.50,175.35,172.65,174.22,45234567";
    
    var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = new StringContent(csvResponse, Encoding.UTF8, "text/csv")
    };

    _mockHttpMessageHandler
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(mockResponse);

    var result = await _stockApiService.GetStockQuoteAsync(stockSymbol);

    Assert.True(result.IsSuccess);
    Assert.Equal(stockSymbol, result.StockSymbol);
    Assert.Equal(174.22m, result.Price);
    Assert.Equal("AAPL.US quote is $174.22 per share", result.FormattedMessage);
    Assert.Null(result.ErrorMessage);
  }

  [Fact]
  public async Task GetStockQuoteAsync_InvalidStockSymbol_ReturnsErrorResult()
  {
    var stockSymbol = "INVALID";
    var csvResponse = "Symbol,Date,Time,Open,High,Low,Close,Volume\nINVALID,N/D,N/D,N/D,N/D,N/D,N/D,N/D";
    
    var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = new StringContent(csvResponse, Encoding.UTF8, "text/csv")
    };

    _mockHttpMessageHandler
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(mockResponse);

    var result = await _stockApiService.GetStockQuoteAsync(stockSymbol);

    Assert.False(result.IsSuccess);
    Assert.Equal(stockSymbol, result.StockSymbol);
    Assert.Null(result.Price);
    Assert.Equal("INVALID quote is not available at this time.", result.FormattedMessage);
    Assert.Equal("Stock price not available (N/D)", result.ErrorMessage);
  }

  [Fact]
  public async Task GetStockQuoteAsync_HttpError_ReturnsErrorResult()
  {
    var stockSymbol = "AAPL.US";
    
    var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
    {
      ReasonPhrase = "Internal Server Error"
    };

    _mockHttpMessageHandler
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(mockResponse);

    var result = await _stockApiService.GetStockQuoteAsync(stockSymbol);

    Assert.False(result.IsSuccess);
    Assert.Equal(stockSymbol, result.StockSymbol);
    Assert.Null(result.Price);
    Assert.Equal("AAPL.US quote is not available at this time.", result.FormattedMessage);
    Assert.Contains("Stock API returned InternalServerError", result.ErrorMessage);
  }

  [Fact]
  public async Task GetStockQuoteAsync_EmptyResponse_ReturnsErrorResult()
  {
    var stockSymbol = "AAPL.US";
    var csvResponse = "";
    
    var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = new StringContent(csvResponse, Encoding.UTF8, "text/csv")
    };

    _mockHttpMessageHandler
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(mockResponse);

    var result = await _stockApiService.GetStockQuoteAsync(stockSymbol);

    Assert.False(result.IsSuccess);
    Assert.Equal(stockSymbol, result.StockSymbol);
    Assert.Null(result.Price);
    Assert.Equal("AAPL.US quote is not available at this time.", result.FormattedMessage);
    Assert.Equal("Empty CSV response from stock API", result.ErrorMessage);
  }

  private void Dispose()
  {
    _httpClient?.Dispose();
  }
}
