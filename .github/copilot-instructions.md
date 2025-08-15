# Chat Challenge .NET - Copilot Instructions

## Project Overview
This is a **real-time chat application** with .NET 8 API backend and React frontend demo interface.

### Repository Structure & Context Detection
When working in:
- `backend/` folder → Apply .NET/C# conventions and patterns
- `frontend/` folder → Apply React/JavaScript conventions and patterns  
- Root/docs → Apply general project documentation standards

## .NET Backend Instructions (backend/ folder)

### Architecture & Dependencies
- Clean Architecture: Api → Core ← Infrastructure
- SignalR for real-time communication
- RabbitMQ for stock bot messaging
- PostgreSQL with EF Core
- xUnit for testing

### Code Conventions
```csharp
// Use meaningful names and async patterns
public async Task<IActionResult> SendMessage(string roomId, string content)
{
  // 2-space indentation
  if (string.IsNullOrEmpty(content))
  {
    return BadRequest("Content is required")
  }
  
  // Single responsibility functions
  await _chatService.SendMessageAsync(roomId, content)
  return Ok()
}
```

### Entity Framework Patterns
```csharp
// Entities in Core layer
public class ChatMessage  
{
  public int Id { get; set; }
  public string Content { get; set; } = string.Empty
  public DateTime CreatedAt { get; set; }
  public string UserId { get; set; } = string.Empty
}
```

### Stock Bot Flow
1. `/stock=AAPL.US` command via SignalR
2. **NOT saved to database** - queued in RabbitMQ
3. Bot processes: `https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv`
4. Response: `"AAPL.US quote is $93.42 per share"`

## React Frontend Instructions (frontend/ folder)

### Technology Stack
- React 18 with functional components
- Material UI v5 for components
- SignalR client for real-time chat
- Axios for HTTP requests

### Code Conventions  
```javascript
// Use meaningful names, 2-space indentation
const ChatRoom = ({ roomId }) => {
  const [messages, setMessages] = useState([])
  const { user } = useAuth()
  
  // Single quotes, no semicolons
  const handleSendMessage = async (content) => {
    if (!content.trim()) return
    
    await signalRService.sendMessage(roomId, content)
  }
  
  return (
    <Box sx={{ p: 2 }}>
      {/* Material UI components */}
    </Box>
  )
}
```

### Custom Hooks Pattern
```javascript
// Extract SignalR logic to reusable hooks
const useSignalR = () => {
  const [connection, setConnection] = useState(null)
  const [messages, setMessages] = useState([])
  
  useEffect(() => {
    // Connection setup with cleanup
  }, [])
  
  return { connection, messages, sendMessage }
}
```

### Service Integration
```javascript
// SignalR connection to backend
const signalRService = new SignalRService()
await signalRService.startConnection(token)
signalRService.onMessageReceived((user, message) => {
  // Handle real-time messages
})
```

## Cross-Layer Integration

### API Endpoints
```csharp
// Backend controller
[HttpGet("rooms/{roomId}/messages")]
public async Task<IActionResult> GetMessages(string roomId)
```

```javascript
// Frontend service call
const getMessages = async (roomId) => {
  const response = await axios.get(`/api/chat/rooms/${roomId}/messages`)
  return response.data
}
```

### SignalR Communication
```csharp
// Backend hub
public async Task SendMessage(string roomId, string message)
{
  await Clients.Group(roomId).SendAsync("ReceiveMessage", Context.User.Identity.Name, message)
}
```

```javascript
// Frontend client
connection.on('ReceiveMessage', (user, message) => {
  setMessages(prev => [...prev, { user, message, timestamp: new Date() }])
})
```

## Development Commands

### Backend (.NET)
```bash
cd backend
dotnet run --project ChatChallenge.Api
dotnet test
dotnet ef migrations add <Name> --project ChatChallenge.Infrastructure --startup-project ChatChallenge.Api
```

### Frontend (React)  
```bash
cd frontend
npm install
npm start
npm test
npm run build
```

## File Context Clues
When suggesting code:
- `.cs` files → Use C# patterns and .NET conventions
- `.jsx/.js` files → Use React patterns and JavaScript conventions
- `Controllers/` → API controller patterns
- `components/` → React component patterns
- `services/` → Service layer patterns (both backend and frontend)
- `*.test.js` or `*.Tests.cs` → Testing patterns for respective platforms

## Critical Workflows
- **Stock bot commands**: Cross both backend RabbitMQ and frontend display
- **Real-time chat**: SignalR hub (backend) ↔ SignalR client (frontend)
- **Authentication**: JWT tokens generated in .NET, consumed in React
- **Message persistence**: EF Core (backend), REST API calls (frontend)