import React, { useState } from 'react'
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Drawer,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import {
  Menu as MenuIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { useChat } from '../hooks/useChat'
import { RoomsList, MessageList, MessageInput } from '../components'

const ChatRoomPage: React.FC = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const [mobileDrawerOpen, setMobileDrawerOpen] = useState(false)
  
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

  const handleMobileRoomSelect = (roomId: number) => {
    selectRoom(roomId)
    setMobileDrawerOpen(false) // Close drawer after selection on mobile
  }

  const handleMobileBackToRooms = () => {
    if (isMobile) {
      selectRoom(null)
    }
  }

  const toggleMobileDrawer = () => {
    setMobileDrawerOpen(!mobileDrawerOpen)
  }

  return (
    <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Top Navigation Bar */}
      <AppBar position="static" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar>
          {/* Mobile menu button or back button */}
          {isMobile && (
            <IconButton
              color="inherit"
              edge="start"
              onClick={selectedRoom ? handleMobileBackToRooms : toggleMobileDrawer}
              sx={{ mr: 2 }}
            >
              {selectedRoom ? <ArrowBackIcon /> : <MenuIcon />}
            </IconButton>
          )}
          
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            {isMobile && selectedRoom ? selectedRoom.name : 'Chat Challenge'}
          </Typography>
          
          {/* Connection Status Indicator */}
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              mr: 2,
              px: 1,
              py: 0.5,
              borderRadius: 1,
              bgcolor: isConnected ? 'success.dark' : 'error.dark',
              color: 'white'
            }}
          >
            <Box
              sx={{
                width: 8,
                height: 8,
                borderRadius: '50%',
                bgcolor: isConnected ? 'success.light' : 'error.light',
                animation: isConnected ? 'pulse 2s infinite' : 'none',
                '@keyframes pulse': {
                  '0%': { opacity: 1 },
                  '50%': { opacity: 0.5 },
                  '100%': { opacity: 1 }
                }
              }}
            />
            <Typography variant="caption" sx={{ display: { xs: 'none', sm: 'block' } }}>
              {isConnected ? 'Connected' : 'Disconnected'}
            </Typography>
          </Box>
          
          <Typography variant="body2" sx={{ mr: 2, display: { xs: 'none', sm: 'block' } }}>
            Welcome, {user?.userName}!
          </Typography>
          
          <Button color="inherit" onClick={handleLogout}>
            Logout
          </Button>
        </Toolbar>
      </AppBar>

      {/* Main Chat Interface - Responsive Layout */}
      <Box sx={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        {/* Desktop: Left Column - Rooms List */}
        {!isMobile && (
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
        )}

        {/* Mobile: Drawer for Rooms List */}
        {isMobile && (
          <Drawer
            variant="temporary"
            anchor="left"
            open={mobileDrawerOpen}
            onClose={() => setMobileDrawerOpen(false)}
            ModalProps={{
              keepMounted: true, // Better open performance on mobile
            }}
            sx={{
              display: { xs: 'block', md: 'none' },
              '& .MuiDrawer-paper': { 
                width: 280,
                pt: 8, // Account for AppBar height
              },
            }}
          >
            <RoomsList
              rooms={rooms}
              selectedRoomId={selectedRoom?.id ?? null}
              isLoading={isLoadingRooms}
              error={roomsError}
              isConnected={isConnected}
              onSelectRoom={handleMobileRoomSelect}
              onCreateRoom={createRoom}
            />
          </Drawer>
        )}

        {/* Center & Bottom Column - Chat Window */}
        <Box
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            overflow: 'hidden',
            // On mobile, hide chat when no room selected
            ...(isMobile && !selectedRoom && { display: 'none' }),
          }}
        >
          {selectedRoom ? (
            <>
              {/* Desktop Chat Header - Mobile shows room name in AppBar */}
              {!isMobile && (
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
              )}

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
            /* No Room Selected State - Only show on desktop or when drawer is closed on mobile */
            (!isMobile || !mobileDrawerOpen) && (
              <Box
                sx={{
                  flex: 1,
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'text.secondary',
                  p: 3,
                }}
              >
                <Typography variant="h5" gutterBottom align="center">
                  Welcome to Chat Challenge
                </Typography>
                <Typography variant="body1" align="center">
                  {isMobile 
                    ? 'Tap the menu button to select a room and start chatting'
                    : 'Select a room from the sidebar to start chatting'
                  }
                </Typography>
              </Box>
            )
          )}
        </Box>
      </Box>
    </Box>
  )
}

export default ChatRoomPage
