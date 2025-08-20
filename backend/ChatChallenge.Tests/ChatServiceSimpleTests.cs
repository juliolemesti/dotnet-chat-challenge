using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.Services;
using ChatChallenge.Core.Entities;
using ChatChallenge.Core.Interfaces;

namespace ChatChallenge.Tests;

/// <summary>
/// Simple tests for ChatService focusing on business logic without complex SignalR mocking
/// </summary>
public class ChatServiceSimpleTests
{
  private readonly Mock<IChatRepository> _mockChatRepository;
  private readonly Mock<ISignalRNotificationService> _mockSignalRService;
  private readonly Mock<ILogger<ChatService>> _mockLogger;
  private readonly IChatService _chatService;

  public ChatServiceSimpleTests()
  {
    _mockChatRepository = new Mock<IChatRepository>();
    _mockSignalRService = new Mock<ISignalRNotificationService>();
    _mockLogger = new Mock<ILogger<ChatService>>();

    _chatService = new ChatService(
      _mockChatRepository.Object,
      _mockSignalRService.Object,
      _mockLogger.Object
    );
  }

  [Fact]
  public async Task GetAllRoomsAsync_ShouldReturnSuccess_WhenRoomsExist()
  {
    // Arrange
    var rooms = new List<ChatRoom>
    {
      new ChatRoom { Id = 1, Name = "General" },
      new ChatRoom { Id = 2, Name = "Random" }
    };
    _mockChatRepository.Setup(r => r.GetAllRoomsAsync()).ReturnsAsync(rooms);

    // Act
    var result = await _chatService.GetAllRoomsAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Data!.Count);
    Assert.Equal("General", result.Data[0].Name);
    Assert.Equal("Random", result.Data[1].Name);
  }

  [Fact]
  public async Task GetAllRoomsAsync_ShouldReturnFailure_WhenExceptionThrown()
  {
    // Arrange
    _mockChatRepository.Setup(r => r.GetAllRoomsAsync())
      .ThrowsAsync(new Exception("Database error"));

    // Act
    var result = await _chatService.GetAllRoomsAsync();

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Failed to retrieve chat rooms", result.ErrorMessage);
    Assert.Equal("GET_ROOMS_ERROR", result.ErrorCode);
  }

  [Fact]
  public async Task GetLastMessagesAsync_ShouldReturnSuccess_WithValidCount()
  {
    // Arrange
    var messages = new List<ChatMessage>
    {
      new ChatMessage { Id = 1, Content = "Hello", UserName = "User1", ChatRoomId = 1 },
      new ChatMessage { Id = 2, Content = "Hi", UserName = "User2", ChatRoomId = 1 }
    };
    _mockChatRepository.Setup(r => r.GetLastMessagesAsync(1, 50)).ReturnsAsync(messages);

    // Act
    var result = await _chatService.GetLastMessagesAsync(1, 50);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Data!.Count);
    Assert.Equal("Hello", result.Data[0].Content);
    Assert.Equal("Hi", result.Data[1].Content);
  }

  [Fact]
  public async Task GetLastMessagesAsync_ShouldReturnFailure_WithInvalidCount()
  {
    // Act
    var result = await _chatService.GetLastMessagesAsync(1, 0);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Count must be between 1 and 100", result.ErrorMessage);
    Assert.Equal("INVALID_COUNT", result.ErrorCode);
  }

  [Fact]
  public async Task SendMessageAsync_ShouldReturnFailure_WithEmptyContent()
  {
    // Act
    var result = await _chatService.SendMessageAsync(1, "", "testuser");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Message content cannot be empty", result.ErrorMessage);
    Assert.Equal("EMPTY_CONTENT", result.ErrorCode);
  }

  [Fact]
  public async Task SendMessageAsync_ShouldReturnFailure_WithEmptyUsername()
  {
    // Act
    var result = await _chatService.SendMessageAsync(1, "Hello", "");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Username is required", result.ErrorMessage);
    Assert.Equal("MISSING_USERNAME", result.ErrorCode);
  }

  [Fact]
  public async Task CreateRoomAsync_ShouldReturnFailure_WithEmptyName()
  {
    // Act
    var result = await _chatService.CreateRoomAsync("");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Room name cannot be empty", result.ErrorMessage);
    Assert.Equal("EMPTY_NAME", result.ErrorCode);
  }
}
