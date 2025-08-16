# Chat Challenge Frontend

Real-time chat application frontend built with React 18, TypeScript, Material UI, and SignalR for seamless communication.

## Overview

This React TypeScript application provides a modern chat interface with real-time messaging capabilities. It connects to a .NET 8 API backend and supports features like:

- **Real-time messaging** via SignalR
- **Stock bot commands** (`/stock=SYMBOL.EXCHANGE`) 
- **Multiple chat rooms** with room management
- **JWT authentication** with automatic token handling
- **Material UI components** for consistent design
- **TypeScript** for type safety and better development experience

## Technology Stack

- **React 18** - Functional components with hooks
- **TypeScript** - Type safety and enhanced developer experience
- **Material UI v5** (`@mui/material`) - Component library and theming
- **Emotion** (`@emotion/react`, `@emotion/styled`) - CSS-in-JS styling for MUI
- **SignalR Client** (`@microsoft/signalr`) - Real-time communication
- **Axios** - HTTP client for API calls
- **React Router** - Client-side routing

## Prerequisites

- Node.js 18+ and npm
- Running .NET backend API (see `../backend/README.md`)
- PostgreSQL database (configured in backend)
- RabbitMQ (for stock bot functionality)

## Getting Started

1. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure environment**
   ```bash
   cp .env.example .env.local
   # Edit .env.local with your API URL
   ```

4. **Start development server**
   ```bash
   npm start
   ```
   Opens [http://localhost:3000](http://localhost:3000)

5. **Ensure backend is running**
   ```bash
   # In ../backend directory
   dotnet run --project ChatChallenge.Api
   ```

## Available Scripts

### `npm start`
Runs the app in development mode with hot reload and lint error display.

### `npm test`
Launches the test runner in interactive watch mode.

### `npm run build`
Builds the app for production to the `build` folder with optimized bundles.

### `npm run eject`
**One-way operation** - Exposes webpack configuration for advanced customization.

## Project Structure

```
frontend/
├── src/
│   ├── components/     # Reusable UI components
│   ├── pages/          # Route components (LoginPage, ChatRoomPage)
│   ├── services/       # API and SignalR services
│   │   ├── authService.ts      # Authentication API calls
│   │   ├── chatService.ts      # Chat REST API calls  
│   │   └── signalRService.ts   # Real-time SignalR client
│   ├── hooks/          # Custom React hooks
│   ├── contexts/       # React contexts (AuthContext)
│   ├── types/          # TypeScript type definitions
│   │   ├── index.ts    # Main type exports
│   │   ├── auth.ts     # Authentication types
│   │   ├── chat.ts     # Chat-related types
│   │   ├── signalr.ts  # SignalR DTOs
│   │   ├── api.ts      # API request/response types
│   │   └── ui.ts       # UI state management types
│   └── util/           # Helper functions and utilities
│       ├── authUtils.ts # Authentication utilities
│       └── consts.ts   # Configuration constants
├── public/             # Static assets
├── package.json        # Dependencies and scripts
└── tsconfig.json       # TypeScript configuration
```

## Key Features

### TypeScript Integration
Full TypeScript support with organized type definitions:

```typescript
// Import types by category
import { User, AuthResponse } from '../types/auth'
import { ChatMessage, ChatRoom } from '../types/chat'
import { SignalRMessageDto } from '../types/signalr'

// Or import everything
import { User, ChatMessage, SignalRMessageDto } from '../types'
```

### Real-time Chat with SignalR
```typescript
import signalRService from '../services/signalRService'

// Setup SignalR connection with authentication
const useSignalR = () => {
  const [isConnected, setIsConnected] = useState(false)
  
  useEffect(() => {
    signalRService.setCallbacks({
      onConnected: (data) => {
        console.log('Connected:', data.message)
        setIsConnected(true)
      },
      onMessageReceived: (message) => {
        setMessages(prev => [...prev, message])
      },
      onAuthenticationFailed: () => {
        // Automatic redirect to login handled
      }
    })
    
    signalRService.startConnection()
  }, [])
  
  return { isConnected }
}
```

### Authentication & Error Handling
Centralized authentication with automatic token management:

```typescript
// Automatic token validation and redirect on auth failures
import { handleAuthenticationError, hasValidToken } from '../util/authUtils'

// Services handle authentication automatically
const rooms = await chatApi.getRooms() // Auto-redirects if token invalid
await signalRService.sendMessage(roomId, message) // Auth handled seamlessly
```

### Stock Bot Commands
Send `/stock=AAPL.US` to get real-time stock quotes processed by the backend bot.

### Material UI with TypeScript
Strongly typed Material UI components with proper theming:

```typescript
import { Box, TextField, Button, Typography } from '@mui/material'
import { styled } from '@mui/material/styles'

const StyledChatBox = styled(Box)(({ theme }) => ({
  padding: theme.spacing(2),
  backgroundColor: theme.palette.background.paper,
  borderRadius: theme.shape.borderRadius
}))

interface ChatRoomProps {
  roomId: string
}

const ChatRoom: React.FC<ChatRoomProps> = ({ roomId }) => {
  const [message, setMessage] = useState<string>('')
  
  const handleSendMessage = async (content: string): Promise<void> => {
    if (!content.trim()) return
    await signalRService.sendMessage(Number(roomId), content)
  }
  
  return (
    <StyledChatBox>
      <Typography variant="h4" gutterBottom>
        Chat Room
      </Typography>
      <TextField
        fullWidth
        value={message}
        onChange={(e) => setMessage(e.target.value)}
        placeholder="Type your message..."
        variant="outlined"
      />
      <Button 
        variant="contained" 
        color="primary"
        onClick={() => handleSendMessage(message)}
      >
        Send
      </Button>
    </StyledChatBox>
  )
}
```

## API Integration

The frontend connects to these backend endpoints with full TypeScript support:

```typescript
// REST API endpoints
- GET /api/chat/rooms                    # Get all chat rooms
- GET /api/chat/rooms/{roomId}/messages  # Fetch message history  
- POST /api/chat/rooms                   # Create new room
- POST /api/chat/rooms/{roomId}/messages # Send message (fallback)
- POST /api/auth/login                   # User authentication
- POST /api/auth/register                # User registration

// SignalR Hub: /chathub - Real-time messaging
- SendMessage(roomId, message)   # Send real-time message
- JoinRoom(roomId)              # Join room group
- LeaveRoom(roomId)             # Leave room group

// SignalR Events (client receives)
- ReceiveMessage(messageDto)     # New message received
- UserJoined(presenceDto)       # User joined room
- UserLeft(presenceDto)         # User left room  
- RoomCreated(roomDto)          # New room created
- Error(errorDto)               # Error notifications
```

### Service Architecture

```typescript
// Centralized API services with automatic authentication
import { chatApi } from '../services/chatService'
import signalRService from '../services/signalRService'

// REST API calls
const rooms: ChatRoom[] = await chatApi.getRooms()
const messages: ChatMessage[] = await chatApi.getMessages(roomId)
const newRoom: ChatRoom = await chatApi.createRoom({ name: 'General' })

// Real-time SignalR
await signalRService.startConnection()
await signalRService.joinRoom(roomId)
await signalRService.sendMessage(roomId, 'Hello World!')
```

## Development Workflow

1. **TypeScript Development**
   - Use functional components with proper TypeScript typing
   - Define interfaces for all props, state, and function parameters
   - Import types from organized type modules (`auth`, `chat`, `signalr`, etc.)

2. **Component Development**
   ```typescript
   interface MessageListProps {
     messages: ChatMessage[]
     onMessageClick: (message: ChatMessage) => void
   }
   
   const MessageList: React.FC<MessageListProps> = ({ messages, onMessageClick }) => {
     return (
       <Box>
         {messages.map((message) => (
           <MessageItem 
             key={message.id} 
             message={message} 
             onClick={() => onMessageClick(message)}
           />
         ))}
       </Box>
     )
   }
   ```

3. **Service Layer Integration**
   - Use centralized services for API calls and SignalR
   - Services handle authentication and error cases automatically
   - Custom hooks for reusable service logic

4. **Authentication Handling**
   - Automatic token validation and refresh
   - Seamless redirect to login on authentication failures
   - No manual token checks needed in components

5. **Material UI with TypeScript**
   ```typescript
   const theme = createTheme({
     palette: {
       primary: {
         main: '#1976d2'
       },
       secondary: {
         main: '#dc004e'
       }
     }
   })
   
   // Typed theme usage
   const useStyles = () => ({
     chatContainer: {
       padding: theme.spacing(2),
       backgroundColor: theme.palette.background.paper
     }
   })
   ```

6. **Type-Safe Testing**
   ```bash
   npm test
   # Type checking
   npm run type-check
   # Specific test file
   npm test ChatRoom.test.tsx
   ```

## Environment Variables

```bash
# .env.local
REACT_APP_API_URL=http://localhost:5016
```

The application will automatically configure:
- API Base URL: `${REACT_APP_API_URL}/api`  
- SignalR Hub URL: `${REACT_APP_API_URL}/chathub`

## Architecture Highlights

### Organized Type System
- **Domain-driven type organization** in separate files
- **Backward compatibility** with main types index
- **Shared authentication utilities** across services
- **SignalR DTOs** matching backend models exactly

### Service Architecture  
- **Centralized authentication** with automatic error handling
- **Axios interceptors** for global token management
- **SignalR service** with connection management and auto-reconnect
- **Error boundaries** and authentication redirects

### Component Architecture
- **TypeScript-first** development approach  
- **Material UI integration** with custom theming
- **Custom hooks** for reusable business logic
- **Context providers** for global state management

## Deployment

1. **Build production bundle**
   ```bash
   npm run build
   ```

2. **Serve static files**
   The `build` folder contains optimized static files ready for deployment to any web server.

## Learn More

- [React Documentation](https://reactjs.org/)
- [TypeScript React Documentation](https://react-typescript-cheatsheet.netlify.app/)
- [Material UI Documentation](https://mui.com/)
- [SignalR JavaScript Client](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [Create React App TypeScript](https://create-react-app.dev/docs/adding-typescript/)

## Recent Updates

- ✅ **Full TypeScript integration** with organized type system
- ✅ **Modular type definitions** organized by domain (auth, chat, signalr, api, ui)  
- ✅ **Enhanced authentication handling** with automatic redirects
- ✅ **Centralized error handling** and token management
- ✅ **SignalR service** with connection management and auto-reconnect
- ✅ **Clean service architecture** with separation of concerns
- ✅ **Material UI integration** optimized for TypeScript