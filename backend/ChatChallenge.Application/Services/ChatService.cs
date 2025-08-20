using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ChatChallenge.Application.Common;
using ChatChallenge.Application.DTOs;
using ChatChallenge.Application.Extensions;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;

namespace ChatChallenge.Application.Services;

/// <summary>
/// Application service for chat-related operations
/// </summary>
public class ChatService : IChatService
{
  private readonly IChatRepository _chatRepository;
  private readonly ISignalRNotificationService _signalRService;
  private readonly ILogger<ChatService> _logger;

  public ChatService(
    IChatRepository chatRepository,
    ISignalRNotificationService signalRService,
    ILogger<ChatService> logger)
  {
    _chatRepository = chatRepository;
    _signalRService = signalRService;
    _logger = logger;
  }

  /// <summary>
  /// Get all available chat rooms
  /// </summary>
  public async Task<ApplicationResult<List<ChatRoom>>> GetAllRoomsAsync()
  {
    try
    {
      var rooms = await _chatRepository.GetAllRoomsAsync();
      return ApplicationResult<List<ChatRoom>>.Success(rooms);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retrieve chat rooms");
      return ApplicationResult<List<ChatRoom>>.Failure("Failed to retrieve chat rooms", "GET_ROOMS_ERROR");
    }
  }

  /// <summary>
  /// Get the last messages from a specific room
  /// </summary>
  public async Task<ApplicationResult<List<ChatMessage>>> GetLastMessagesAsync(int roomId, int count = 50)
  {
    try
    {
      if (count <= 0 || count > 100)
      {
        return ApplicationResult<List<ChatMessage>>.Failure("Count must be between 1 and 100", "INVALID_COUNT");
      }

      var messages = await _chatRepository.GetLastMessagesAsync(roomId, count);
      return ApplicationResult<List<ChatMessage>>.Success(messages);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retrieve messages for room {RoomId}", roomId);
      return ApplicationResult<List<ChatMessage>>.Failure("Failed to retrieve messages", "GET_MESSAGES_ERROR");
    }
  }

  /// <summary>
  /// Send a message to a room and broadcast it
  /// </summary>
  public async Task<ApplicationResult<ChatMessage>> SendMessageAsync(int roomId, string content, string userName)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(content))
      {
        return ApplicationResult<ChatMessage>.Failure("Message content cannot be empty", "EMPTY_CONTENT");
      }

      if (string.IsNullOrWhiteSpace(userName))
      {
        return ApplicationResult<ChatMessage>.Failure("Username is required", "MISSING_USERNAME");
      }

      var message = new ChatMessage
      {
        Content = content.Trim(),
        UserName = userName,
        ChatRoomId = roomId,
        IsStockBot = false
      };

      var savedMessage = await _chatRepository.AddMessageAsync(message);
      
      _logger.LogDebug("Message saved: ID={Id}, RoomId={RoomId}, User={User}", 
        savedMessage.Id, savedMessage.ChatRoomId, savedMessage.UserName);

      var messageDto = savedMessage.ToSignalRDto();
      await _signalRService.SendMessageToRoomAsync(roomId, messageDto);
      
      _logger.LogDebug("Message broadcasted to room {RoomId}", roomId);

      return ApplicationResult<ChatMessage>.Success(savedMessage);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send message to room {RoomId}", roomId);
      return ApplicationResult<ChatMessage>.Failure("Failed to send message", "SEND_MESSAGE_ERROR");
    }
  }

  /// <summary>
  /// Create a new chat room
  /// </summary>
  public async Task<ApplicationResult<ChatRoom>> CreateRoomAsync(string name)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        return ApplicationResult<ChatRoom>.Failure("Room name cannot be empty", "EMPTY_NAME");
      }

      var room = new ChatRoom
      {
        Name = name.Trim()
      };

      var savedRoom = await _chatRepository.CreateRoomAsync(room);
      
      _logger.LogInformation("Room created: ID={Id}, Name={Name}", savedRoom.Id, savedRoom.Name);

      var roomDto = savedRoom.ToSignalRDto(memberCount: 0);
      await _signalRService.BroadcastRoomCreatedAsync(roomDto);
      
      _logger.LogDebug("Room creation broadcasted: {Name}", savedRoom.Name);

      return ApplicationResult<ChatRoom>.Success(savedRoom);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create room: {Name}", name);
      return ApplicationResult<ChatRoom>.Failure("Failed to create room", "CREATE_ROOM_ERROR");
    }
  }
}
