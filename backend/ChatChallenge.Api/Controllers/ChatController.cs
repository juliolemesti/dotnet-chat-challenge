using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;

namespace ChatChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
  private readonly IChatRepository _chatRepository;

  public ChatController(IChatRepository chatRepository)
  {
    _chatRepository = chatRepository;
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

    var message = new ChatMessage
    {
      Content = request.Content,
      UserName = userName,
      ChatRoomId = roomId,
      IsStockBot = false
    };

    var savedMessage = await _chatRepository.AddMessageAsync(message);
    return CreatedAtAction(nameof(GetMessages), new { roomId }, savedMessage);
  }

  [HttpPost("rooms")]
  public async Task<ActionResult<ChatRoom>> CreateRoom([FromBody] CreateRoomRequest request)
  {
    var room = new ChatRoom
    {
      Name = request.Name
    };

    var savedRoom = await _chatRepository.CreateRoomAsync(room);
    return CreatedAtAction(nameof(GetRooms), savedRoom);
  }
}

public record SendMessageRequest(string Content);
public record CreateRoomRequest(string Name);
