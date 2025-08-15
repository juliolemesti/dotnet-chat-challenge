# Chat Challenge .NET

A real-time chat application built with .NET 8 backend API and React TypeScript frontend demo interface, featuring stock quote bot integration using SignalR, RabbitMQ, and PostgreSQL.

## Project Overview

This is a **full-stack chat application** that demonstrates modern .NET web development practices with real-time communication and message broker integration. The backend follows Clean Architecture principles with distinct layers for separation of concerns, while the frontend provides a React TypeScript-based demo interface.

### Key Technologies
- **.NET 8** - Modern web API framework
- **ASP.NET Core Web API** - REST API with SignalR integration
- **SignalR** - Real-time bidirectional communication
- **RabbitMQ** - Message broker for decoupled bot operations
- **PostgreSQL** - Primary database with Entity Framework Core
- **Stock API Integration** - Real-time stock quotes via stooq.com
- **React 18 + TypeScript** - Frontend demo interface with Material UI

## Architecture

The project follows a **backend/frontend** structure with Clean Architecture patterns:

```
chat-challenge-dotnet/
├── backend/                    # .NET 8 API Backend
│   ├── ChatChallenge.Api/      # Presentation Layer (API)
│   │   ├── Controllers/        # REST API controllers
│   │   ├── Hubs/              # SignalR hubs for real-time communication
│   │   ├── Middleware/        # Custom middleware
│   │   └── Program.cs         # Application entry point
│   ├── ChatChallenge.Core/     # Domain Layer
│   │   ├── Entities/          # Domain entities
│   │   ├── Interfaces/        # Repository and service contracts
│   │   └── Services/          # Business logic services
│   ├── ChatChallenge.Infrastructure/ # Infrastructure Layer
│   │   ├── Data/              # EF Core DbContext and configurations
│   │   ├── Repositories/      # Data access implementations
│   │   ├── Services/          # External service integrations
│   │   └── Messaging/         # RabbitMQ message handling
│   └── ChatChallenge.Tests/    # Test Layer
│       ├── Unit/              # Unit tests
│       └── Integration/       # Integration tests
├── frontend/                   # React TypeScript Frontend Demo
│   ├── src/
│   │   ├── components/        # React TypeScript components
│   │   ├── services/          # API and SignalR services
│   │   ├── hooks/             # Custom React hooks
│   │   ├── types/             # TypeScript type definitions
│   │   └── App.tsx            # Main application component
│   ├── public/
│   ├── package.json
│   └── tsconfig.json          # TypeScript configuration
└── README.md
```

## Prerequisites

Before running the project, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (v12 or higher)
- [RabbitMQ](https://www.rabbitmq.com/download.html)
- [Git](https://git-scm.com/downloads)

## Setup and Installation

### 1. Clone the Repository
```bash
git clone <repository-url>
cd chat-challenge-dotnet
```

### 2. Backend Setup
```bash
cd backend

# Start PostgreSQL service
# Create a database named 'chatdb'

# Apply Entity Framework migrations
dotnet ef database update --project ChatChallenge.Infrastructure --startup-project ChatChallenge.Api
```

### 3. Frontend Setup
```bash
cd frontend

# Install dependencies
npm install

# Install additional dependencies for SignalR and Material UI
npm install @microsoft/signalr @mui/material @emotion/react @emotion/styled
npm install @types/node --save-dev
```

### 4. RabbitMQ Setup
```bash
# Start RabbitMQ service
# Default management UI: http://localhost:15672 (guest/guest)
```

### 5. Configuration
Update `appsettings.Development.json` in `backend/ChatChallenge.Api` with your connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chatdb;Username=your_user;Password=your_password"
  },
  "RabbitMQ": {
    "Hostname": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## Running the Application

### Development Mode
```bash
# Run the backend API (opens Swagger UI at https://localhost:7147)
cd backend
dotnet run --project ChatChallenge.Api

# Run the frontend (opens at http://localhost:3000)
cd frontend
npm start

# Run backend tests
cd backend
dotnet test
```

### Access Points
- **Frontend Demo**: http://localhost:3000
- **API Documentation**: https://localhost:7147/swagger
- **SignalR Hub**: https://localhost:7147/chathub
- **RabbitMQ Management**: http://localhost:15672

## API Features

The **ChatChallenge.Api** project provides:

- **User Authentication** - JWT-based authentication endpoints
- **Chat API** - REST endpoints for chat room and message management
- **SignalR Hub** - Real-time messaging with `/chathub` endpoint
- **Stock Commands** - Process `/stock=AAPL.US` commands via message broker
- **Message History** - Retrieve last 50 messages with timestamps

### Testing the API
1. **Start the backend API** (`cd backend && dotnet run --project ChatChallenge.Api`)
2. **Start the frontend demo** (`cd frontend && npm start`)
3. **Open Swagger UI** at https://localhost:7147/swagger for API testing
4. **Use the React demo** at http://localhost:3000 for interactive testing
5. **Test stock commands** by sending `/stock=AAPL.US` messages through the chat interface

## API Usage

### Authentication Endpoints
```http
POST /api/auth/register
POST /api/auth/login
```

### Chat Endpoints
```http
GET /api/chat/rooms
GET /api/chat/rooms/{roomId}/messages
POST /api/chat/rooms/{roomId}/messages
```

### SignalR Hub
- **Hub URL**: `/chathub`
- **Methods**: `SendMessage`, `JoinRoom`, `LeaveRoom`

## Stock Bot Commands

Users can retrieve stock quotes by sending commands via SignalR:

```
/stock=AAPL.US
```

**Workflow:**
1. User sends stock command via SignalR
2. Command is queued in RabbitMQ (not saved to database)
3. Bot service processes stock API call
4. Bot sends formatted response: `"AAPL.US quote is $93.42 per share"`

## Frontend Integration

The included React TypeScript frontend demonstrates:
- **JWT Authentication** integration with the .NET API
- **SignalR Client** for real-time chat functionality
- **Material UI Components** for modern user interface
- **Custom Hooks** for state management and API integration
- **TypeScript** for type safety and better development experience

Any frontend technology can integrate with this API using:
- **HTTP REST calls** for authentication and message management
- **SignalR JavaScript client** for real-time chat functionality

Example SignalR connection in TypeScript:
```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7147/chathub")
    .build();
```

## Testing

### Running Tests
```bash
# Run backend tests
cd backend
dotnet test

# Run specific test project
dotnet test ChatChallenge.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run frontend tests
cd frontend
npm test
```

### Test Scenarios
- **Multi-user chat**: Use multiple browser tabs with the React demo to test real-time messaging
- **Stock commands**: Test `/stock=AAPL.US` command functionality via the frontend interface
- **Message ordering**: Verify last 50 messages display correctly in the React UI

## Development Commands

### Backend (.NET)
```bash
cd backend

# Add new migration
dotnet ef migrations add <MigrationName> --project ChatChallenge.Infrastructure --startup-project ChatChallenge.Api

# Update database
dotnet ef database update --project ChatChallenge.Infrastructure --startup-project ChatChallenge.Api

# Build solution
dotnet build

# Restore packages
dotnet restore
```

### Frontend (React + TypeScript)
```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm start

# Run tests
npm test

# Build for production
npm run build

# Type check
npx tsc --noEmit
```

---

# Project Instructions

## Description
This project is designed to test your knowledge of **back-end web technologies**, specifically in **.NET**, and assess your ability to create back-end products with attention to **details, standards, and reusability**.

## Assignment
The goal of this exercise is to create a **chat API application** using .NET that provides real-time communication capabilities and stock quote integration.

## Mandatory Features
- **User Authentication:** JWT-based authentication for API access
- **Stock Command:** Allow users to post messages as commands using the following format: /stock=stock_code

- **Decoupled Bot:**  
  - Call an API using the `stock_code` as a parameter:  
    `https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv` (here `aapl.us` is the stock code)  
  - Parse the received CSV file and send a message back into the chatroom using a **message broker** like RabbitMQ.  
  - The message format should be:  
    ```
    AAPL.US quote is $93.42 per share
    ```  
  - The post owner should be the bot.  
- **Message Ordering:** Show messages ordered by timestamps and display only the **last 50 messages**.  
- **Unit Tests:** Test at least one functionality of your choice.

## Bonus (Optional)
- Support **multiple chatrooms**.  
- Use **.NET Identity** for user authentication.  
- Handle messages that are **not understood** or any exceptions raised by the bot.  
- Build an **installer**.

## Considerations
- Tests can be performed using the **React demo interface**, **SignalR clients**, or **API testing tools**.  
- **Stock commands will not be saved** in the database as a post.  
- Project includes both **backend API** and **frontend demo** components.  
- Keep **confidential information secure**.  
- Monitor resource usage to avoid excessive consumption.  
- Keep your code **versioned with Git** locally.  
- Small helper libraries may be used if needed.
