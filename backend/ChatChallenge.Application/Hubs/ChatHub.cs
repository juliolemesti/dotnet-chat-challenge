using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using ChatChallenge.Application.DTOs;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.Extensions;

namespace ChatChallenge.Application.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality
/// </summary>
[Authorize]
public class ChatHub : Hub, IChatHub
{
  private readonly IChatService _chatService;
  private readonly IStockBotService _stockBotService;
  private readonly ILogger<ChatHub> _logger;

  public ChatHub(
    IChatService chatService, 
    IStockBotService stockBotService,
    ILogger<ChatHub> logger)
  {
    _chatService = chatService;
    _stockBotService = stockBotService;
    _logger = logger;
  }

  /// <summary>
  /// Send a message to a specific chat room
  /// </summary>
  /// <param name="roomId">The ID of the chat room</param>
  /// <param name="message">The message content</param>
  public async Task SendMessage(string roomId, string message)
  {
    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(userName))
    {
      var errorDto = EntityExtensions.CreateErrorDto("Authentication required", "AUTH_REQUIRED");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    if (string.IsNullOrWhiteSpace(message))
    {
      var errorDto = EntityExtensions.CreateErrorDto("Message cannot be empty", "EMPTY_MESSAGE");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    if (!int.TryParse(roomId, out int roomIdInt))
    {
      var errorDto = EntityExtensions.CreateErrorDto("Invalid room ID", "INVALID_ROOM_ID");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    if (IsStockCommand(message))
    {
      await HandleStockCommand(roomId, message, userName);
      return;
    }

    try
    {
      var result = await _chatService.SendMessageAsync(roomIdInt, message, userName);
      
      if (!result.IsSuccess)
      {
        _logger.LogWarning("Failed to send message: {Error}", result.ErrorMessage);
        var errorDto = EntityExtensions.CreateErrorDto(result.ErrorMessage, result.ErrorCode);
        await Clients.Caller.SendAsync("Error", errorDto);
        return;
      }

      _logger.LogDebug("✅ Message sent successfully via ChatService for Room_{RoomId}", roomId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send message to Room_{RoomId}", roomId);
      var errorDto = EntityExtensions.CreateErrorDto("Failed to send message", "SEND_MESSAGE_ERROR");
      await Clients.Caller.SendAsync("Error", errorDto);
    }
  }

  /// <summary>
  /// Join a chat room group
  /// </summary>
  /// <param name="roomId">The ID of the chat room to join</param>
  public async Task JoinRoom(string roomId)
  {
    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(userName))
    {
      var errorDto = new SignalRErrorDto
      {
        Message = "Authentication required",
        Code = "AUTH_REQUIRED"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    if (!int.TryParse(roomId, out int roomIdInt))
    {
      var errorDto = new SignalRErrorDto
      {
        Message = "Invalid room ID",
        Code = "INVALID_ROOM_ID"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    var roomsResult = await _chatService.GetAllRoomsAsync();
    if (!roomsResult.IsSuccess || !roomsResult.Data!.Any(r => r.Id == roomIdInt))
    {
      var errorDto = new SignalRErrorDto
      {
        Message = "Room not found",
        Code = "ROOM_NOT_FOUND"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    _logger.LogDebug("🏠 User {UserName} (ConnectionId: {ConnectionId}) joined group Room_{RoomId}", 
      userName, Context.ConnectionId, roomId);
    
    var presenceDto = new SignalRUserPresenceDto
    {
      UserName = userName,
      RoomId = roomId
    };
    
    await Clients.OthersInGroup($"Room_{roomId}").SendAsync("UserJoined", presenceDto);
    
    await Clients.Caller.SendAsync("JoinedRoom", roomId);
    _logger.LogDebug("🏠 User {UserName} successfully joined room {RoomId}", userName, roomId);
  }

  /// <summary>
  /// Leave a chat room group
  /// </summary>
  /// <param name="roomId">The ID of the chat room to leave</param>
  public async Task LeaveRoom(string roomId)
  {
    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(userName))
    {
      var errorDto = new SignalRErrorDto
      {
        Message = "Authentication required",
        Code = "AUTH_REQUIRED"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    
    var presenceDto = new SignalRUserPresenceDto
    {
      UserName = userName,
      RoomId = roomId
    };
    
    await Clients.OthersInGroup($"Room_{roomId}").SendAsync("UserLeft", presenceDto);
    
    await Clients.Caller.SendAsync("LeftRoom", roomId);
  }

  /// <summary>
  /// Handle client connection
  /// </summary>
  public override async Task OnConnectedAsync()
  {
    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    if (!string.IsNullOrEmpty(userName))
    {
      var connectionDto = new SignalRConnectionDto
      {
        UserName = userName,
        Message = $"Welcome {userName}!"
      };
      await Clients.Caller.SendAsync("Connected", connectionDto);
    }
    await base.OnConnectedAsync();
  }

  /// <summary>
  /// Handle client disconnection
  /// </summary>
  /// <param name="exception">The exception that caused the disconnection, if any</param>
  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    await base.OnDisconnectedAsync(exception);
  }

  /// <summary>
  /// Check if a message is a stock command
  /// </summary>
  /// <param name="message">The message to check</param>
  /// <returns>True if the message is a stock command</returns>
  private bool IsStockCommand(string message)
  {
    return message.StartsWith("/stock=", StringComparison.OrdinalIgnoreCase) && 
           !string.IsNullOrEmpty(_stockBotService.ExtractStockSymbol(message));
  }

  /// <summary>
  /// Handle stock bot commands using real message broker
  /// </summary>
  /// <param name="roomId">The room ID where the command was sent</param>
  /// <param name="command">The stock command</param>
  /// <param name="userName">The user who sent the command</param>
  private async Task HandleStockCommand(string roomId, string command, string userName)
  {
    try
    {
      var stockSymbol = _stockBotService.ExtractStockSymbol(command);
      
      if (string.IsNullOrEmpty(stockSymbol))
      {
        var errorDto = EntityExtensions.CreateErrorDto("Invalid stock command format. Use: /stock=SYMBOL", "INVALID_STOCK_COMMAND");
        await Clients.Caller.SendAsync("Error", errorDto);
        return;
      }

      await _stockBotService.QueueStockRequestAsync(stockSymbol, userName, roomId);

      var ackMessage = new SignalRMessageDto
      {
        Id = Random.Shared.Next(100000, 999999),
        Content = $"Stock request for {stockSymbol} is being processed...",
        UserName = "StockBot",
        RoomId = int.Parse(roomId),
        CreatedAt = DateTime.UtcNow,
        IsStockBot = true
      };

      await Clients.Caller.SendAsync("ReceiveMessage", ackMessage);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to process stock command in Room_{RoomId}", roomId);
      var errorDto = EntityExtensions.CreateErrorDto("Failed to process stock command", "STOCK_COMMAND_ERROR");
      await Clients.Caller.SendAsync("Error", errorDto);
    }
  }
}
