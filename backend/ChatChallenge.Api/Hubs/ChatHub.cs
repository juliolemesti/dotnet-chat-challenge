using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;
using ChatChallenge.Api.Models;
using ChatChallenge.Api.Extensions;

namespace ChatChallenge.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
  private readonly IChatRepository _chatRepository;

  public ChatHub(IChatRepository chatRepository)
  {
    _chatRepository = chatRepository;
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
      var errorDto = new SignalRErrorDto
      {
        Message = "Authentication required",
        Code = "AUTH_REQUIRED"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    if (string.IsNullOrWhiteSpace(message))
    {
      var errorDto = new SignalRErrorDto
      {
        Message = "Message cannot be empty",
        Code = "EMPTY_MESSAGE"
      };
      await Clients.Caller.SendAsync("Error", errorDto);
      return;
    }

    // Parse roomId to integer
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

    // Create and save the message to database
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
      
      // Convert to SignalR DTO and broadcast
      var messageDto = savedMessage.ToSignalRDto();
      await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", messageDto);
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
}
