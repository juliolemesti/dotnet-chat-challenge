using ChatChallenge.Application.Common;
using ChatChallenge.Application.DTOs;
using ChatChallenge.Core.Entities;

namespace ChatChallenge.Application.Interfaces;

/// <summary>
/// Interface for chat-related application operations
/// </summary>
public interface IChatService
{
  /// <summary>
  /// Get all available chat rooms
  /// </summary>
  /// <returns>List of chat rooms</returns>
  Task<ApplicationResult<List<ChatRoom>>> GetAllRoomsAsync();

  /// <summary>
  /// Get the last messages from a specific room
  /// </summary>
  /// <param name="roomId">Room ID</param>
  /// <param name="count">Number of messages to retrieve</param>
  /// <returns>List of messages</returns>
  Task<ApplicationResult<List<ChatMessage>>> GetLastMessagesAsync(int roomId, int count = 50);

  /// <summary>
  /// Send a message to a room and broadcast it
  /// </summary>
  /// <param name="roomId">Room ID</param>
  /// <param name="content">Message content</param>
  /// <param name="userName">User sending the message</param>
  /// <returns>The sent message</returns>
  Task<ApplicationResult<ChatMessage>> SendMessageAsync(int roomId, string content, string userName);

  /// <summary>
  /// Create a new chat room
  /// </summary>
  /// <param name="name">Room name</param>
  /// <returns>The created room</returns>
  Task<ApplicationResult<ChatRoom>> CreateRoomAsync(string name);
}
