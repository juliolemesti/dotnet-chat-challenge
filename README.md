# Chat Challenge .NET

A real-time chat application built with .NET 8 backend API and React TypeScript frontend. 

## Project Overview

The backend follows Clean Architecture principles with layers for separation of concerns, while the frontend provides a React TypeScript-based demo interface.

### Key Technologies
- **.NET 8** - Modern web API framework
- **ASP.NET Core Web API** - REST API with SignalR integration
- **SignalR** - Real-time bidirectional communication
- **InMemory Message Broker** - Lightweight message handling for decoupled bot operations
- **SQLite** - Lightweight database with Entity Framework Core
- **Stock API Integration** - Real-time stock quotes via stooq.com
- **React 18 + TypeScript** - Frontend demo interface with Material UI

## Architecture

The project follows a **backend/frontend** structure with Clean Architecture patterns.

## Prerequisites

Before running the project, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22](https://nodejs.org/en/download/) -
- [Git](https://git-scm.com/downloads)

## Setup and Installation

### 1. Clone the Repository
```bash
git clone https://github.com/juliolemesti/dotnet-chat-challenge
cd dotnet-chat-challenge
```

### 2. Frontend Setup
```bash
cd frontend

# Install dependencies
npm install
```

## Running the Application

### Development Mode
```bash
# Run the backend API (opens Swagger UI at http://localhost:5016)
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
- **API Documentation**: https://localhost:5016/swagger
- **SignalR Hub**: https://localhost:5016/chathub

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
```
