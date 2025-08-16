using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;
using ChatChallenge.Api.Models;
using ChatChallenge.Api.Extensions;
using ChatChallenge.Api.Services;

namespace ChatChallenge.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
  private readonly IChatRepository _chatRepository;
  private readonly IStockBotService _stockBotService;

  public ChatHub(IChatRepository chatRepository, IStockBotService stockBotService)
  {
    _chatRepository = chatRepository;
    _stockBotService = stockBotService;
  }

  /// <summary>
  /// Send a message to a specific chat room
  /// </summary>
  /// <param name="roomId">The ID of the chat room</param>
  /// <param name="message">The message content</param>
  public async Task SendMessage(string roomId, string message)
  {
    // Get the username from JWT claims
    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(userName))
    {
      var errorDto = SignalRExtensions.CreateErrorDto("Authentication required", "AUTH_REQUIRED");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    if (string.IsNullOrWhiteSpace(message))
    {
      var errorDto = SignalRExtensions.CreateErrorDto("Message cannot be empty", "EMPTY_MESSAGE");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    // Parse roomId to integer
    if (!int.TryParse(roomId, out int roomIdInt))
    {
      var errorDto = SignalRExtensions.CreateErrorDto("Invalid room ID", "INVALID_ROOM_ID");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    // Check if this is a stock bot command
    if (IsStockCommand(message))
    {
      await HandleStockCommand(roomId, message, userName);
      return;
    }

    // Create and save the regular message to database
    var chatMessage = new ChatMessage
    {
      Content = message,
      UserName = userName,
      ChatRoomId = roomIdInt,
      IsStockBot = false
    };

    try
    {
      var savedMessage = await _chatRepository.AddMessageAsync(chatMessage);
      
      Console.WriteLine($"💾 Message saved to DB: ID={savedMessage.Id}, RoomId={savedMessage.ChatRoomId}, User={savedMessage.UserName}");
      
      // Convert to SignalR DTO and broadcast
      var messageDto = savedMessage.ToSignalRDto();
      Console.WriteLine($"📡 Broadcasting message to group Room_{roomId}: {System.Text.Json.JsonSerializer.Serialize(messageDto)}");
      
      await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", messageDto);
      Console.WriteLine($"📡 Message broadcast completed for Room_{roomId}");
    }
    catch (Exception)
    {
      var errorDto = SignalRExtensions.CreateErrorDto("Failed to send message", "SEND_MESSAGE_ERROR");
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

    // Parse roomId to integer for validation
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

    // Verify room exists
    var rooms = await _chatRepository.GetAllRoomsAsync();
    if (!rooms.Any(r => r.Id == roomIdInt))
    {
      var errorDto = new SignalRErrorDto
      {
        Message = "Room not found",
        Code = "ROOM_NOT_FOUND"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    // Join the room group
    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    Console.WriteLine($"🏠 User {userName} (ConnectionId: {Context.ConnectionId}) joined group Room_{roomId}");
    
    // Create presence DTO for user joined notification
    var presenceDto = new SignalRUserPresenceDto
    {
      UserName = userName,
      RoomId = roomId
    };
    
    // Notify others in the room that user joined
    await Clients.OthersInGroup($"Room_{roomId}").SendAsync("UserJoined", presenceDto);
    
    // Confirm to the caller that they joined successfully
    await Clients.Caller.SendAsync("JoinedRoom", roomId);
    Console.WriteLine($"🏠 User {userName} successfully joined room {roomId}");
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

    // Remove from the room group
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    
    // Create presence DTO for user left notification
    var presenceDto = new SignalRUserPresenceDto
    {
      UserName = userName,
      RoomId = roomId
    };
    
    // Notify others in the room that user left
    await Clients.OthersInGroup($"Room_{roomId}").SendAsync("UserLeft", presenceDto);
    
    // Confirm to the caller that they left successfully
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
    // Clean up any room memberships if needed
    // In a production app, you might want to track user presence
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
  /// Handle stock bot commands (preparation for future RabbitMQ integration)
  /// </summary>
  /// <param name="roomId">The room ID where the command was sent</param>
  /// <param name="command">The stock command</param>
  /// <param name="userName">The user who sent the command</param>
  private async Task HandleStockCommand(string roomId, string command, string userName)
  {
    try
    {
      // Extract stock symbol using the service
      var stockSymbol = _stockBotService.ExtractStockSymbol(command);
      
      if (string.IsNullOrEmpty(stockSymbol))
      {
        var errorDto = SignalRExtensions.CreateErrorDto("Invalid stock command format. Use: /stock=SYMBOL", "INVALID_STOCK_COMMAND");
        await Clients.Caller.SendAsync("Error", errorDto);
        return;
      }

      // Queue the stock request (future RabbitMQ implementation)
      await _stockBotService.QueueStockRequestAsync(stockSymbol, userName, roomId);

      // Send placeholder response for now
      await SendStockBotPlaceholderResponse(roomId, stockSymbol, userName);
    }
    catch (Exception)
    {
      var errorDto = SignalRExtensions.CreateErrorDto("Failed to process stock command", "STOCK_COMMAND_ERROR");
      await Clients.Caller.SendAsync("Error", errorDto);
    }
  }

  /// <summary>
  /// Send a placeholder stock bot response (temporary implementation)
  /// </summary>
  /// <param name="roomId">The room ID</param>
  /// <param name="stockSymbol">The requested stock symbol</param>
  /// <param name="requestingUser">The user who requested the stock info</param>
  private async Task SendStockBotPlaceholderResponse(string roomId, string stockSymbol, string requestingUser)
  {
    // Create a placeholder bot response message
    var botMessage = new SignalRMessageDto
    {
      Id = 0, // Temporary ID since this won't be saved to database yet
      Content = $"{stockSymbol} quote is not available yet. Stock bot service is not implemented.",
      UserName = "StockBot",
      RoomId = int.Parse(roomId),
      CreatedAt = DateTime.UtcNow,
      IsStockBot = true
    };

    // Broadcast the bot response to all clients in the room
    await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", botMessage);

    // TODO: Remove this placeholder when RabbitMQ integration is implemented
    // The actual flow will be:
    // 1. Queue stock request in RabbitMQ
    // 2. External bot service processes queue
    // 3. Bot service calls stooq.com API
    // 4. Bot service sends response back via SignalR hub
  }
}
