import React from 'react'
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Button,
} from '@mui/material'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { useChat } from '../hooks/useChat'
import { RoomsList, MessageList, MessageInput } from '../components'

const ChatRoomPage: React.FC = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  
  // Master hook that coordinates all chat functionality
  const {
    // Rooms state
    rooms,
    selectedRoomId,
    isLoadingRooms,
    roomsError,
    
    // Messages state
    messages,
    isLoadingMessages,
    messagesError,
    
    // SignalR state
    isConnected,
    
    // Actions
    selectRoom,
    createRoom,
    sendMessage,
  } = useChat()

  // Find selected room object from ID
  const selectedRoom = selectedRoomId ? rooms.find(r => r.id === selectedRoomId) : null

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const handleSendMessage = async (content: string): Promise<void> => {
    if (!selectedRoom) return
    await sendMessage(content)
  }

  return (
    <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Top Navigation Bar */}
      <AppBar position="static" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            Chat Challenge
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>
            Welcome, {user?.userName}!
          </Typography>
          <Button color="inherit" onClick={handleLogout}>
            Logout
          </Button>
        </Toolbar>
      </AppBar>

      {/* Main Chat Interface - 3 Column Layout */}
      <Box sx={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        {/* Left Column - Rooms List */}
        <Box
          sx={{
            width: 300,
            borderRight: 1,
            borderColor: 'divider',
            display: 'flex',
            flexDirection: 'column',
            backgroundColor: 'background.paper',
          }}
        >
          <RoomsList
            rooms={rooms}
            selectedRoomId={selectedRoom?.id ?? null}
            isLoading={isLoadingRooms}
            error={roomsError}
            isConnected={isConnected}
            onSelectRoom={selectRoom}
            onCreateRoom={createRoom}
          />
        </Box>

        {/* Center & Bottom Column - Chat Window */}
        <Box
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            overflow: 'hidden',
          }}
        >
          {selectedRoom ? (
            <>
              {/* Chat Header */}
              <Box
                sx={{
                  p: 2,
                  borderBottom: 1,
                  borderColor: 'divider',
                  backgroundColor: 'background.paper',
                }}
              >
                <Typography variant="h6">{selectedRoom.name}</Typography>
                <Typography variant="caption" color="text.secondary">
                  Connection: {isConnected ? 'Connected' : 'Disconnected'}
                </Typography>
              </Box>

              {/* Center - Messages List */}
              <Box sx={{ flex: 1, overflow: 'hidden' }}>
                <MessageList
                  messages={messages}
                  isLoading={isLoadingMessages}
                  error={messagesError}
                  selectedRoomName={selectedRoom.name}
                />
              </Box>

              {/* Bottom - Message Input */}
              <Box
                sx={{
                  borderTop: 1,
                  borderColor: 'divider',
                  backgroundColor: 'background.paper',
                }}
              >
                <MessageInput
                  onSendMessage={handleSendMessage}
                  disabled={!isConnected}
                  isConnected={isConnected}
                  placeholder={
                    !isConnected
                      ? 'Connecting...'
                      : `Message ${selectedRoom.name}`
                  }
                />
              </Box>
            </>
          ) : (
            /* No Room Selected State */
            <Box
              sx={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'text.secondary',
              }}
            >
              <Typography variant="h5" gutterBottom>
                Welcome to Chat Challenge
              </Typography>
              <Typography variant="body1">
                Select a room from the sidebar to start chatting
              </Typography>
            </Box>
          )}
        </Box>
      </Box>
    </Box>
  )
}

export default ChatRoomPage
