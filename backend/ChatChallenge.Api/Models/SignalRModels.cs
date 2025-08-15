namespace ChatChallenge.Api.Models;

/// <summary>
/// DTO for chat messages sent via SignalR
/// </summary>
public class SignalRMessageDto
{
  public int Id { get; set; }
  public string Content { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public int RoomId { get; set; }
  public DateTime CreatedAt { get; set; }
  public bool IsStockBot { get; set; }
}

/// <summary>
/// DTO for room information sent via SignalR
/// </summary>
public class SignalRRoomDto
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public int MemberCount { get; set; } = 0;
}

/// <summary>
/// DTO for user presence notifications via SignalR
/// </summary>
public class SignalRUserPresenceDto
{
  public string UserName { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for error notifications via SignalR
/// </summary>
public class SignalRErrorDto
{
  public string Message { get; set; } = string.Empty;
  public string? Code { get; set; }
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for connection status via SignalR
/// </summary>
public class SignalRConnectionDto
{
  public string UserName { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for typing indicators via SignalR (future enhancement)
/// </summary>
public class SignalRTypingDto
{
  public string UserName { get; set; } = string.Empty;
  public string RoomId { get; set; } = string.Empty;
  public bool IsTyping { get; set; }
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for room statistics via SignalR
/// </summary>
public class SignalRRoomStatsDto
{
  public int RoomId { get; set; }
  public int OnlineUsers { get; set; }
  public int MessageCount { get; set; }
  public DateTime LastActivity { get; set; }
}
