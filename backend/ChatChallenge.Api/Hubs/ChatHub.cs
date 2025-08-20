using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Api.Services;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;
using ChatChallenge.Api.Extensions;
using ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
  private readonly IChatRepository _chatRepository;
  private readonly IStockBotService _stockBotService;
  private readonly ChatChallenge.Api.Services.IMessageBrokerService _messageBroker;

  public ChatHub(
    IChatRepository chatRepository, 
    IStockBotService stockBotService,
    ChatChallenge.Api.Services.IMessageBrokerService messageBroker)
  {
    _chatRepository = chatRepository;
    _stockBotService = stockBotService;
    _messageBroker = messageBroker;
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

    if (!int.TryParse(roomId, out int roomIdInt))
    {
      var errorDto = SignalRExtensions.CreateErrorDto("Invalid room ID", "INVALID_ROOM_ID");
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    Console.WriteLine($"📨 Received message in room {roomId} from {userName}: {message}");

    if (IsStockCommand(message))
    {
      Console.WriteLine($"🤖 Detected stock command: {message}");
      await HandleStockCommand(roomId, message, userName);
      return;
    }

    Console.WriteLine($"💬 Processing regular message from {userName}");

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
      
      var messageDto = savedMessage.ToSignalRDto();
      Console.WriteLine($"📡 Broadcasting message to group Room_{roomId}: {System.Text.Json.JsonSerializer.Serialize(messageDto)}");
      
      await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", messageDto);
      Console.WriteLine($"📡 Message broadcast completed for Room_{roomId}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error saving/broadcasting message: {ex.Message}");
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

    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    Console.WriteLine($"🏠 User {userName} (ConnectionId: {Context.ConnectionId}) joined group Room_{roomId}");
    
    // Subscribe to stock responses for this room
    _messageBroker.SubscribeToStockResponses(roomId, async (stockResponse) =>
    {
      Console.WriteLine($"📈 Received stock response in room {roomId}: {stockResponse.FormattedMessage}");
      
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

        Console.WriteLine($"📡 Broadcasting StockBot response to Room_{roomId}: {stockResponse.FormattedMessage}");
        await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", botMessage);
        Console.WriteLine($"✅ StockBot response broadcast completed for Room_{roomId}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error broadcasting StockBot response to Room_{roomId}: {ex.Message}");
      }
    });
    
    Console.WriteLine($"🔔 Subscribed to stock responses for Room_{roomId}");
    
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

    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    
    _messageBroker.UnsubscribeFromStockResponses(roomId);
    
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
    var isCommand = message.StartsWith("/stock=", StringComparison.OrdinalIgnoreCase);
    var hasValidSymbol = isCommand && !string.IsNullOrEmpty(_stockBotService.ExtractStockSymbol(message));
    
    Console.WriteLine($"🔍 Checking if '{message}' is stock command: starts with /stock={isCommand}, has valid symbol={hasValidSymbol}");
    
    return hasValidSymbol;
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
      Console.WriteLine($"🤖 Processing stock command in room {roomId}: {command} from {userName}");
      
      var stockSymbol = _stockBotService.ExtractStockSymbol(command);
      
      if (string.IsNullOrEmpty(stockSymbol))
      {
        Console.WriteLine($"❌ Invalid stock symbol in command: {command}");
        var errorDto = SignalRExtensions.CreateErrorDto("Invalid stock command format. Use: /stock=SYMBOL", "INVALID_STOCK_COMMAND");
        await Clients.Caller.SendAsync("Error", errorDto);
        return;
      }

      Console.WriteLine($"📝 Extracted stock symbol: {stockSymbol}");
      Console.WriteLine($"📤 Queueing stock request for {stockSymbol} in room {roomId}");
      
      await _stockBotService.QueueStockRequestAsync(stockSymbol, userName, roomId);
      
      Console.WriteLine($"✅ Stock request queued successfully for {stockSymbol}");

      var ackMessage = new SignalRMessageDto
      {
        Id = Random.Shared.Next(100000, 999999),
        Content = $"Stock request for {stockSymbol} is being processed...",
        UserName = "StockBot",
        RoomId = int.Parse(roomId),
        CreatedAt = DateTime.UtcNow,
        IsStockBot = true
      };

      Console.WriteLine($"📨 Sending acknowledgment message to caller for {stockSymbol}");
      await Clients.Caller.SendAsync("ReceiveMessage", ackMessage);
      Console.WriteLine($"✅ Acknowledgment sent for {stockSymbol}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error processing stock command '{command}' in room {roomId}: {ex.Message}");
      var errorDto = SignalRExtensions.CreateErrorDto("Failed to process stock command", "STOCK_COMMAND_ERROR");
      await Clients.Caller.SendAsync("Error", errorDto);
    }
  }
}
