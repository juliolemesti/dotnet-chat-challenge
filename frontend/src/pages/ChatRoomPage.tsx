import React from 'react'
import {
  Container,
  Paper,
  Typography,
  Box,
  AppBar,
  Toolbar,
  Button,
} from '@mui/material'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'

const ChatRoomPage: React.FC = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            Chat Challenge
          </Typography>
          <Typography variant="body1" sx={{ mr: 2 }}>
            Welcome, {user?.userName}!
          </Typography>
          <Button color="inherit" onClick={handleLogout}>
            Logout
          </Button>
        </Toolbar>
      </AppBar>
      
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Paper elevation={3} sx={{ p: 4 }}>
          <Typography variant="h4" gutterBottom>
            Chat Room
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Welcome to the chat room! This is where the chat functionality will be implemented.
          </Typography>
          <Box sx={{ mt: 2 }}>
            <Typography variant="body2">
              User: {user?.email} ({user?.userName})
            </Typography>
            <Typography variant="body2">
              User ID: {user?.id}
            </Typography>
          </Box>
        </Paper>
      </Container>
    </>
  )
}

export default ChatRoomPage
