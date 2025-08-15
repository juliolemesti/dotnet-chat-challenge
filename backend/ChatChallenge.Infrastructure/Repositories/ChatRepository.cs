using Microsoft.EntityFrameworkCore;
using ChatChallenge.Core.Entities;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Data;

namespace ChatChallenge.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
  private readonly ChatDbContext _context;

  public ChatRepository(ChatDbContext context)
  {
    _context = context;
  }

  public async Task<List<ChatMessage>> GetLastMessagesAsync(int chatRoomId, int count = 50)
  {
    return await _context.ChatMessages
      .Where(m => m.ChatRoomId == chatRoomId)
      .OrderByDescending(m => m.CreatedAt)
      .Take(count)
      .OrderBy(m => m.CreatedAt)
      .ToListAsync();
  }

  public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
  {
    message.CreatedAt = DateTime.UtcNow;
    _context.ChatMessages.Add(message);
    await _context.SaveChangesAsync();
    return message;
  }

  public async Task<List<ChatRoom>> GetAllRoomsAsync()
  {
    return await _context.ChatRooms
      .OrderBy(r => r.Name)
      .ToListAsync();
  }

  public async Task<ChatRoom?> GetRoomByIdAsync(int id)
  {
    return await _context.ChatRooms
      .Include(r => r.Messages)
      .FirstOrDefaultAsync(r => r.Id == id);
  }

  public async Task<ChatRoom> CreateRoomAsync(ChatRoom room)
  {
    room.CreatedAt = DateTime.UtcNow;
    _context.ChatRooms.Add(room);
    await _context.SaveChangesAsync();
    return room;
  }
}
