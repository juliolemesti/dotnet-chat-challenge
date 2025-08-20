namespace ChatChallenge.Application.Interfaces;

/// <summary>
/// Interface for Chat Hub operations
/// </summary>
public interface IChatHub
{
  /// <summary>
  /// Send a message to a specific chat room
  /// </summary>
  /// <param name="roomId">The ID of the chat room</param>
  /// <param name="message">The message content</param>
  Task SendMessage(string roomId, string message);

  /// <summary>
  /// Join a chat room group
  /// </summary>
  /// <param name="roomId">The ID of the chat room to join</param>
  Task JoinRoom(string roomId);

  /// <summary>
  /// Leave a chat room group
  /// </summary>
  /// <param name="roomId">The ID of the chat room to leave</param>
  Task LeaveRoom(string roomId);
}
