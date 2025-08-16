using System.Globalization;

namespace ChatChallenge.Api.Services;

/// <summary>
/// Service for fetching stock quotes from the Stooq API
/// </summary>
public interface IStockApiService
{
  /// <summary>
  /// Fetch stock quote for the given symbol
  /// </summary>
  /// <param name="stockSymbol">The stock symbol (e.g., "AAPL.US")</param>
  /// <returns>Stock quote result with price or error information</returns>
  Task<StockQuoteResult> GetStockQuoteAsync(string stockSymbol);
}

/// <summary>
/// Result of a stock quote API call
/// </summary>
public class StockQuoteResult
{
  public bool IsSuccess { get; set; }
  public string StockSymbol { get; set; } = string.Empty;
  public decimal? Price { get; set; }
  public string FormattedMessage { get; set; } = string.Empty;
  public string? ErrorMessage { get; set; }
}

/// <summary>
/// Implementation of stock API service using Stooq.com
/// </summary>
public class StockApiService : IStockApiService
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<StockApiService> _logger;
  private const string StooqApiUrlTemplate = "https://stooq.com/q/l/?s={0}&f=sd2t2ohlcv&h&e=csv";

  public StockApiService(HttpClient httpClient, ILogger<StockApiService> logger)
  {
    _httpClient = httpClient;
    _logger = logger;
    
    // Set timeout for API calls
    _httpClient.Timeout = TimeSpan.FromSeconds(10);
  }

  public async Task<StockQuoteResult> GetStockQuoteAsync(string stockSymbol)
  {
    try
    {
      _logger.LogInformation("Fetching stock quote for symbol: {StockSymbol}", stockSymbol);

      // Construct the API URL
      var apiUrl = string.Format(StooqApiUrlTemplate, stockSymbol.ToLowerInvariant());
      _logger.LogDebug("Stock API URL: {ApiUrl}", apiUrl);

      // Make HTTP request
      var response = await _httpClient.GetAsync(apiUrl);

      if (!response.IsSuccessStatusCode)
      {
        var errorMessage = $"Stock API returned {response.StatusCode}: {response.ReasonPhrase}";
        _logger.LogWarning("Stock API request failed: {ErrorMessage}", errorMessage);
        
        return new StockQuoteResult
        {
          IsSuccess = false,
          StockSymbol = stockSymbol,
          ErrorMessage = errorMessage,
          FormattedMessage = $"{stockSymbol} quote is not available at this time."
        };
      }

      // Read CSV response
      var csvContent = await response.Content.ReadAsStringAsync();
      _logger.LogDebug("Stock API CSV response: {CsvContent}", csvContent);

      // Parse CSV and extract price
      var parseResult = ParseStockCsv(csvContent, stockSymbol);
      
      if (parseResult.IsSuccess && parseResult.Price.HasValue)
      {
        _logger.LogInformation("Successfully fetched stock quote: {StockSymbol} = ${Price:F2}", 
          stockSymbol, parseResult.Price.Value);
      }
      else
      {
        _logger.LogWarning("Failed to parse stock quote for {StockSymbol}: {ErrorMessage}", 
          stockSymbol, parseResult.ErrorMessage);
      }

      return parseResult;
    }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
    {
      var errorMessage = "Stock API request timed out";
      _logger.LogWarning("Stock API timeout for symbol {StockSymbol}: {ErrorMessage}", stockSymbol, errorMessage);
      
      return new StockQuoteResult
      {
        IsSuccess = false,
        StockSymbol = stockSymbol,
        ErrorMessage = errorMessage,
        FormattedMessage = $"{stockSymbol} quote is not available at this time."
      };
    }
    catch (HttpRequestException ex)
    {
      var errorMessage = $"Network error while fetching stock quote: {ex.Message}";
      _logger.LogError(ex, "Network error for stock symbol {StockSymbol}", stockSymbol);
      
      return new StockQuoteResult
      {
        IsSuccess = false,
        StockSymbol = stockSymbol,
        ErrorMessage = errorMessage,
        FormattedMessage = $"{stockSymbol} quote is not available at this time."
      };
    }
    catch (Exception ex)
    {
      var errorMessage = $"Unexpected error while fetching stock quote: {ex.Message}";
      _logger.LogError(ex, "Unexpected error for stock symbol {StockSymbol}", stockSymbol);
      
      return new StockQuoteResult
      {
        IsSuccess = false,
        StockSymbol = stockSymbol,
        ErrorMessage = errorMessage,
        FormattedMessage = $"{stockSymbol} quote is not available at this time."
      };
    }
  }

  /// <summary>
  /// Parse CSV response from Stooq API and extract stock price
  /// Expected CSV format: Symbol,Date,Time,Open,High,Low,Close,Volume
  /// Example: AAPL.US,2023-10-20,22:00:01,173.50,175.35,172.65,174.22,45234567
  /// </summary>
  private StockQuoteResult ParseStockCsv(string csvContent, string stockSymbol)
  {
    if (string.IsNullOrWhiteSpace(csvContent))
    {
      return new StockQuoteResult
      {
        IsSuccess = false,
        StockSymbol = stockSymbol,
        ErrorMessage = "Empty CSV response from stock API",
        FormattedMessage = $"{stockSymbol} quote is not available at this time."
      };
    }

    try
    {
      var lines = csvContent.Trim().Split('\n');
      
      if (lines.Length < 2)
      {
        return new StockQuoteResult
        {
          IsSuccess = false,
          StockSymbol = stockSymbol,
          ErrorMessage = "Invalid CSV format: insufficient data rows",
          FormattedMessage = $"{stockSymbol} quote is not available at this time."
        };
      }

      // Skip header row and get the data row
      var dataLine = lines[1].Trim();
      var columns = dataLine.Split(',');

      // Expected format: Symbol,Date,Time,Open,High,Low,Close,Volume
      if (columns.Length < 7)
      {
        return new StockQuoteResult
        {
          IsSuccess = false,
          StockSymbol = stockSymbol,
          ErrorMessage = "Invalid CSV format: insufficient columns",
          FormattedMessage = $"{stockSymbol} quote is not available at this time."
        };
      }

      // Extract close price (index 6)
      var closePriceString = columns[6].Trim();
      
      if (string.IsNullOrEmpty(closePriceString) || closePriceString.ToLowerInvariant() == "n/d")
      {
        return new StockQuoteResult
        {
          IsSuccess = false,
          StockSymbol = stockSymbol,
          ErrorMessage = "Stock price not available (N/D)",
          FormattedMessage = $"{stockSymbol} quote is not available at this time."
        };
      }

      // Parse the price
      if (!decimal.TryParse(closePriceString, NumberStyles.Float, CultureInfo.InvariantCulture, out var price))
      {
        return new StockQuoteResult
        {
          IsSuccess = false,
          StockSymbol = stockSymbol,
          ErrorMessage = $"Unable to parse stock price: {closePriceString}",
          FormattedMessage = $"{stockSymbol} quote is not available at this time."
        };
      }

      // Create successful result with formatted message
      var formattedMessage = $"{stockSymbol.ToUpperInvariant()} quote is ${price:F2} per share";
      
      return new StockQuoteResult
      {
        IsSuccess = true,
        StockSymbol = stockSymbol,
        Price = price,
        FormattedMessage = formattedMessage
      };
    }
    catch (Exception ex)
    {
      return new StockQuoteResult
      {
        IsSuccess = false,
        StockSymbol = stockSymbol,
        ErrorMessage = $"CSV parsing error: {ex.Message}",
        FormattedMessage = $"{stockSymbol} quote is not available at this time."
      };
    }
  }
}
