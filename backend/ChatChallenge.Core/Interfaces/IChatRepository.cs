using ChatChallenge.Core.Entities;

namespace ChatChallenge.Core.Interfaces;

public interface IChatRepository
{
  Task<List<ChatMessage>> GetLastMessagesAsync(int chatRoomId, int count = 50);
  Task<ChatMessage> AddMessageAsync(ChatMessage message);
  Task<List<ChatRoom>> GetAllRoomsAsync();
  Task<ChatRoom?> GetRoomByIdAsync(int id);
  Task<ChatRoom> CreateRoomAsync(ChatRoom room);
}
