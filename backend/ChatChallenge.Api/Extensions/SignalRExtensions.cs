using ChatChallenge.Core.Entities;
using ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Extensions;

/// <summary>
/// Extension methods for converting entities to SignalR DTOs
/// </summary>
public static class SignalRExtensions
{
  /// <summary>
  /// Converts ChatMessage entity to SignalR DTO
  /// </summary>
  public static SignalRMessageDto ToSignalRDto(this ChatMessage message)
  {
    return new SignalRMessageDto
    {
      Id = message.Id,
      Content = message.Content,
      UserName = message.UserName,
      RoomId = message.ChatRoomId,
      CreatedAt = message.CreatedAt,
      IsStockBot = message.IsStockBot
    };
  }

  /// <summary>
  /// Converts ChatRoom entity to SignalR DTO
  /// </summary>
  public static SignalRRoomDto ToSignalRDto(this ChatRoom room, int memberCount = 0)
  {
    return new SignalRRoomDto
    {
      Id = room.Id,
      Name = room.Name,
      CreatedAt = room.CreatedAt,
      MemberCount = memberCount
    };
  }

  /// <summary>
  /// Creates a SignalR error DTO
  /// </summary>
  public static SignalRErrorDto CreateErrorDto(string message, string? code = null)
  {
    return new SignalRErrorDto
    {
      Message = message,
      Code = code,
      Timestamp = DateTime.UtcNow
    };
  }

  /// <summary>
  /// Creates a SignalR user presence DTO
  /// </summary>
  public static SignalRUserPresenceDto CreatePresenceDto(string userName, string roomId)
  {
    return new SignalRUserPresenceDto
    {
      UserName = userName,
      RoomId = roomId,
      Timestamp = DateTime.UtcNow
    };
  }

  /// <summary>
  /// Creates a SignalR connection DTO
  /// </summary>
  public static SignalRConnectionDto CreateConnectionDto(string userName, string message)
  {
    return new SignalRConnectionDto
    {
      UserName = userName,
      Message = message,
      ConnectedAt = DateTime.UtcNow
    };
  }
}
