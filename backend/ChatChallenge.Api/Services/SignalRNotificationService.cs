using Microsoft.AspNetCore.SignalR;
using ChatChallenge.Api.Hubs;
using ChatChallenge.Api.Models;
using ChatChallenge.Api.Extensions;

namespace ChatChallenge.Api.Services
{
  public class SignalRNotificationService : ISignalRNotificationService
  {
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
      IHubContext<ChatHub> hubContext,
      ILogger<SignalRNotificationService> logger)
    {
      _hubContext = hubContext;
      _logger = logger;
    }

    public async Task SendStockResponseToRoomAsync(StockResponseMessage stockResponse)
    {
      try
      {
        var botMessage = new SignalRMessageDto
        {
          Id = Random.Shared.Next(100000, 999999),
          Content = stockResponse.FormattedMessage,
          UserName = "StockBot",
          RoomId = int.Parse(stockResponse.RoomId),
          CreatedAt = stockResponse.ResponseAt,
          IsStockBot = true
        };

        // Broadcast the bot response to all clients in the room using IHubContext
        _logger.LogInformation("📈 Sending stock bot response to Room_{RoomId}: {Message}", 
          stockResponse.RoomId, stockResponse.FormattedMessage);
        
        await _hubContext.Clients.Group($"Room_{stockResponse.RoomId}")
          .SendAsync("ReceiveMessage", botMessage);

        _logger.LogInformation("✅ Stock bot response successfully sent to Room_{RoomId}", stockResponse.RoomId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "❌ Failed to send stock bot response to Room_{RoomId}: {Message}", 
          stockResponse.RoomId, ex.Message);
        throw;
      }
    }
  }
}
