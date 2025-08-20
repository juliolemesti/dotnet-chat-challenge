using ChatChallenge.Application.DTOs;

namespace ChatChallenge.Application.Interfaces;

/// <summary>
/// Interface for SignalR notification operations
/// </summary>
public interface ISignalRNotificationService
{
  /// <summary>
  /// Send stock bot response to a specific room
  /// </summary>
  /// <param name="stockResponse">The stock response to send</param>
  /// <returns>Task representing the async operation</returns>
  Task SendStockResponseToRoomAsync(StockResponseMessage stockResponse);

  /// <summary>
  /// Send a chat message to a specific room
  /// </summary>
  /// <param name="roomId">The room ID</param>
  /// <param name="message">The message to send</param>
  /// <returns>Task representing the async operation</returns>
  Task SendMessageToRoomAsync(int roomId, SignalRMessageDto message);

  /// <summary>
  /// Broadcast a room creation notification to all clients
  /// </summary>
  /// <param name="room">The room that was created</param>
  /// <returns>Task representing the async operation</returns>
  Task BroadcastRoomCreatedAsync(SignalRRoomDto room);
}
