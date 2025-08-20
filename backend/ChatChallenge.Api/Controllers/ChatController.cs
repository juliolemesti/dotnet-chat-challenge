using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatChallenge.Core.Entities;
using ChatChallenge.Application.Interfaces;

namespace ChatChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
  private readonly IChatService _chatService;

  public ChatController(IChatService chatService)
  {
    _chatService = chatService;
  }

  [HttpGet("rooms")]
  public async Task<ActionResult<List<ChatRoom>>> GetRooms()
  {
    var result = await _chatService.GetAllRoomsAsync();
    
    if (!result.IsSuccess)
    {
      return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
    }

    return Ok(result.Data);
  }

  [HttpGet("rooms/{roomId}/messages")]
  public async Task<ActionResult<List<ChatMessage>>> GetMessages(int roomId, [FromQuery] int count = 50)
  {
    var result = await _chatService.GetLastMessagesAsync(roomId, count);
    
    if (!result.IsSuccess)
    {
      return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
    }

    return Ok(result.Data);
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

    var result = await _chatService.SendMessageAsync(roomId, request.Content, userName);
    
    if (!result.IsSuccess)
    {
      if (result.ErrorCode == "EMPTY_CONTENT")
        return BadRequest(result.ErrorMessage);
        
      return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
    }

    return CreatedAtAction(nameof(GetMessages), new { roomId }, result.Data);
  }

  [HttpPost("rooms")]
  public async Task<ActionResult<ChatRoom>> CreateRoom([FromBody] CreateRoomRequest request)
  {
    var result = await _chatService.CreateRoomAsync(request.Name);
    
    if (!result.IsSuccess)
    {
      if (result.ErrorCode == "EMPTY_NAME")
        return BadRequest(result.ErrorMessage);
        
      return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
    }

    return CreatedAtAction(nameof(GetRooms), result.Data);
  }
}

public record SendMessageRequest(string Content);
public record CreateRoomRequest(string Name);
