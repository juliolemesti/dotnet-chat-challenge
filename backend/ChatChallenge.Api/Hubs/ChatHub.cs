using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;

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
      await Clients.Caller.SendAsync("Error", "Authentication required");
      return;
    }

    if (string.IsNullOrWhiteSpace(message))
    {
      await Clients.Caller.SendAsync("Error", "Message cannot be empty");
      return;
    }

    // Parse roomId to integer
    if (!int.TryParse(roomId, out int roomIdInt))
    {
      await Clients.Caller.SendAsync("Error", "Invalid room ID");
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
      
      // Broadcast the message to all clients in the room group
      await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", new
      {
        id = savedMessage.Id,
        content = savedMessage.Content,
        userName = savedMessage.UserName,
        roomId = savedMessage.ChatRoomId,
        createdAt = savedMessage.CreatedAt,
        isStockBot = savedMessage.IsStockBot
      });
    }
    catch (Exception ex)
    {
      await Clients.Caller.SendAsync("Error", "Failed to send message");
      // Log the exception in a real application
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
      await Clients.Caller.SendAsync("Error", "Authentication required");
      return;
    }

    // Parse roomId to integer for validation
    if (!int.TryParse(roomId, out int roomIdInt))
    {
      await Clients.Caller.SendAsync("Error", "Invalid room ID");
      return;
    }

    // Verify room exists
    var rooms = await _chatRepository.GetAllRoomsAsync();
    if (!rooms.Any(r => r.Id == roomIdInt))
    {
      await Clients.Caller.SendAsync("Error", "Room not found");
      return;
    }

    // Join the room group
    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    
    // Notify others in the room that user joined
    await Clients.OthersInGroup($"Room_{roomId}").SendAsync("UserJoined", userName, roomId);
    
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
      await Clients.Caller.SendAsync("Error", "Authentication required");
      return;
    }

    // Remove from the room group
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
    
    // Notify others in the room that user left
    await Clients.OthersInGroup($"Room_{roomId}").SendAsync("UserLeft", userName, roomId);
    
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
      await Clients.Caller.SendAsync("Connected", $"Welcome {userName}!");
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
