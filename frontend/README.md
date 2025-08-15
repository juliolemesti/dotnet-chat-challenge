# Chat Challenge Frontend

Real-time chat application frontend built with React 18, Material UI, and SignalR for seamless communication.

## Overview

This React application provides a modern chat interface with real-time messaging capabilities. It connects to a .NET 8 API backend and supports features like:

- Real-time messaging via SignalR
- Stock bot commands (`/stock=SYMBOL.EXCHANGE`)
- Material UI components for consistent design
- JWT authentication
- Multiple chat rooms

## Technology Stack

- **React 18** - Functional components with hooks
- **Material UI v5** (`@mui/material`) - Component library and theming
- **Emotion** (`@emotion/react`, `@emotion/styled`) - CSS-in-JS styling for MUI
- **SignalR Client** - Real-time communication
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
│   ├── pages/          # Route components
│   ├── services/       # API and SignalR services
│   ├── hooks/          # Custom React hooks
│   ├── contexts/       # React contexts (auth, theme)
│   └── utils/          # Helper functions
├── public/             # Static assets
└── package.json        # Dependencies and scripts
```

## Key Features

### Real-time Chat
```javascript
// SignalR integration
const useSignalR = () => {
  const [connection, setConnection] = useState(null)
  
  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl('/chatHub', { accessTokenFactory: () => token })
      .build()
    
    newConnection.start().then(() => setConnection(newConnection))
  }, [])
}
```

### Stock Bot Commands
Send `/stock=AAPL.US` to get real-time stock quotes processed by the backend bot.

### Material UI Integration
Consistent design system with theming support and responsive components using Emotion for styling:

```javascript
import { Box, TextField, Button, Typography } from '@mui/material'
import { styled } from '@mui/material/styles'

const StyledChatBox = styled(Box)(({ theme }) => ({
  padding: theme.spacing(2),
  backgroundColor: theme.palette.background.paper,
  borderRadius: theme.shape.borderRadius
}))

const ChatRoom = () => {
  return (
    <StyledChatBox>
      <Typography variant="h4" gutterBottom>
        Chat Room
      </Typography>
      <TextField
        fullWidth
        placeholder="Type your message..."
        variant="outlined"
      />
      <Button variant="contained" color="primary">
        Send
      </Button>
    </StyledChatBox>
  )
}
```

## API Integration

The frontend connects to these backend endpoints:

- `GET /api/chat/rooms/{roomId}/messages` - Fetch message history
- `POST /api/auth/login` - User authentication
- SignalR Hub: `/chatHub` - Real-time messaging

## Development Workflow

1. **Component Development**
   - Use functional components with hooks
   - Follow Material UI design patterns with Emotion styling
   - Extract custom hooks for reusable logic

2. **Service Layer**
   - API calls in dedicated service files
   - SignalR connection management
   - Error handling and loading states

3. **Material UI Theming**
   ```javascript
   import { createTheme, ThemeProvider } from '@mui/material/styles'
   
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
   ```

4. **Testing**
   ```bash
   npm test
   # Run specific test file
   npm test ChatRoom.test.js
   ```

## Environment Variables

```bash
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_URL=http://localhost:5000/chatHub
```

## Deployment

1. **Build production bundle**
   ```bash
   npm run build
   ```

2. **Serve static files**
   The `build` folder contains optimized static files ready for deployment to any web server.

## Learn More

- [React Documentation](https://reactjs.org/)
- [Material UI Documentation](https://mui.com/)
- [SignalR JavaScript Client](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [Create React App Documentation](https://facebook.github.io/create-react-app/docs/getting-started)