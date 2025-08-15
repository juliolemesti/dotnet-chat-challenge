using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;
using ChatChallenge.Api.Hubs;
using ChatChallenge.Api.Models;
using ChatChallenge.Api.Extensions;

namespace ChatChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
  private readonly IChatRepository _chatRepository;
  private readonly IHubContext<ChatHub> _hubContext;

  public ChatController(IChatRepository chatRepository, IHubContext<ChatHub> hubContext)
  {
    _chatRepository = chatRepository;
    _hubContext = hubContext;
  }

  [HttpGet("rooms")]
  public async Task<ActionResult<List<ChatRoom>>> GetRooms()
  {
    var rooms = await _chatRepository.GetAllRoomsAsync();
    return Ok(rooms);
  }

  [HttpGet("rooms/{roomId}/messages")]
  public async Task<ActionResult<List<ChatMessage>>> GetMessages(int roomId, [FromQuery] int count = 50)
  {
    var messages = await _chatRepository.GetLastMessagesAsync(roomId, count);
    return Ok(messages);
  }

  [HttpPost("rooms/{roomId}/messages")]
  public async Task<ActionResult<ChatMessage>> SendMessage(int roomId, [FromBody] SendMessageRequest request)
  {
    // Get the username from JWT claims
    var userName = User.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(userName))
    {
      return Unauthorized("Invalid token: username not found");
    }

    if (string.IsNullOrWhiteSpace(request.Content))
    {
      return BadRequest("Message content cannot be empty");
    }

    var message = new ChatMessage
    {
      Content = request.Content,
      UserName = userName,
      ChatRoomId = roomId,
      IsStockBot = false
    };

    try
    {
      // Save message to database
      var savedMessage = await _chatRepository.AddMessageAsync(message);

      // Convert to SignalR DTO and broadcast to room
      var messageDto = savedMessage.ToSignalRDto();
      await _hubContext.Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", messageDto);

      return CreatedAtAction(nameof(GetMessages), new { roomId }, savedMessage);
    }
    catch (Exception)
    {
      return StatusCode(500, "Failed to send message");
    }
  }

  [HttpPost("rooms")]
  public async Task<ActionResult<ChatRoom>> CreateRoom([FromBody] CreateRoomRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.Name))
    {
      return BadRequest("Room name cannot be empty");
    }

    var room = new ChatRoom
    {
      Name = request.Name
    };

    try
    {
      // Save room to database
      var savedRoom = await _chatRepository.CreateRoomAsync(room);

      // Convert to SignalR DTO and broadcast to all clients
      var roomDto = savedRoom.ToSignalRDto(memberCount: 0);
      await _hubContext.Clients.All.SendAsync("RoomCreated", roomDto);

      return CreatedAtAction(nameof(GetRooms), savedRoom);
    }
    catch (Exception)
    {
      return StatusCode(500, "Failed to create room");
    }
  }
}

public record SendMessageRequest(string Content);
public record CreateRoomRequest(string Name);
