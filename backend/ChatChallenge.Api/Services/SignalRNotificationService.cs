using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ChatChallenge.Application.DTOs;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Api.Hubs;

namespace ChatChallenge.Api.Services;

/// <summary>
/// Service for sending SignalR notifications
/// </summary>
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

  public async Task SendMessageToRoomAsync(int roomId, SignalRMessageDto message)
  {
    try
    {
      _logger.LogDebug("📤 Sending message to Room_{RoomId}", roomId);
      
      await _hubContext.Clients.Group($"Room_{roomId}")
        .SendAsync("ReceiveMessage", message);

      _logger.LogDebug("✅ Message successfully sent to Room_{RoomId}", roomId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Failed to send message to Room_{RoomId}", roomId);
      throw;
    }
  }

  public async Task BroadcastRoomCreatedAsync(SignalRRoomDto room)
  {
    try
    {
      _logger.LogDebug("📢 Broadcasting room creation: {RoomName}", room.Name);
      
      await _hubContext.Clients.All.SendAsync("RoomCreated", room);

      _logger.LogDebug("✅ Room creation successfully broadcasted: {RoomName}", room.Name);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Failed to broadcast room creation: {RoomName}", room.Name);
      throw;
    }
  }
}
