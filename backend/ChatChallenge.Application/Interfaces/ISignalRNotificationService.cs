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
}
