import React, { useEffect, useRef } from 'react'
import {
  Box,
  Typography,
  Alert,
  Paper
} from '@mui/material'
import { styled } from '@mui/material/styles'
import { ChatMessage } from '../types'
import { MessageItem } from './MessageItem'
import { MessageSkeleton } from './MessageSkeleton'
import { useAuth } from '../contexts/AuthContext'

interface MessageListProps {
  messages: ChatMessage[]
  isLoading: boolean
  error: string | null
  selectedRoomName?: string
}

const ChatContainer = styled(Paper)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  backgroundColor: theme.palette.background.default,
  borderRadius: theme.shape.borderRadius
}))

const ChatHeader = styled(Box)(({ theme }) => ({
  padding: theme.spacing(2),
  borderBottom: `1px solid ${theme.palette.divider}`,
  backgroundColor: theme.palette.background.paper
}))

const MessagesContainer = styled(Box)(({ theme }) => ({
  flex: 1,
  overflowY: 'auto',
  padding: theme.spacing(1),
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing(0.5),
  maxHeight: 'calc(100vh - 280px)', // Adjust based on header and input heights
  [theme.breakpoints.down('sm')]: {
    maxHeight: 'calc(100vh - 120px)', // Reduced for mobile
    padding: theme.spacing(0.5)
  },
  '&::-webkit-scrollbar': {
    width: '8px'
  },
  '&::-webkit-scrollbar-track': {
    background: theme.palette.action.hover
  },
  '&::-webkit-scrollbar-thumb': {
    background: theme.palette.action.disabled,
    borderRadius: '4px'
  },
  '&::-webkit-scrollbar-thumb:hover': {
    background: theme.palette.action.selected
  }
}))

const EmptyState = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  padding: theme.spacing(4),
  flex: 1,
  textAlign: 'center',
  color: theme.palette.text.secondary
}))

export const MessageList: React.FC<MessageListProps> = ({
  messages,
  isLoading,
  error,
  selectedRoomName = 'Chat Room'
}) => {
  const { user } = useAuth()
  const messagesEndRef = useRef<HTMLDivElement>(null)

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth' })
    }
  }, [messages])

  const isCurrentUserMessage = (message: ChatMessage): boolean => {
    return user?.userName === message.userName
  }

  if (error) {
    return (
      <ChatContainer elevation={1}>
        <ChatHeader>
          <Typography variant="h6" color="text.primary">
            {selectedRoomName}
          </Typography>
        </ChatHeader>
        <Box sx={{ p: 2 }}>
          <Alert severity="error" variant="outlined">
            {error}
          </Alert>
        </Box>
      </ChatContainer>
    )
  }

  return (
    <ChatContainer elevation={1}>
      <ChatHeader>
        <Typography variant="h6" color="text.primary">
          {selectedRoomName}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {messages.length > 0 ? `${messages.length} messages` : 'No messages yet'}
        </Typography>
      </ChatHeader>

      <MessagesContainer>
        {isLoading ? (
          <>
            <MessageSkeleton count={3} />
            <MessageSkeleton isCurrentUser count={2} />
            <MessageSkeleton count={1} />
          </>
        ) : messages.length === 0 ? (
          <EmptyState>
            <Typography variant="h6" gutterBottom>
              No messages yet
            </Typography>
            <Typography variant="body2">
              Be the first to send a message in this room!
            </Typography>
            <Typography variant="caption" sx={{ mt: 1 }}>
              💡 Try sending <code>/stock=AAPL.US</code> to get stock quotes
            </Typography>
          </EmptyState>
        ) : (
          <>
            {messages.map((message) => (
              <MessageItem
                key={message.id}
                message={message}
                isCurrentUser={isCurrentUserMessage(message)}
              />
            ))}
            <div ref={messagesEndRef} />
          </>
        )}
      </MessagesContainer>
    </ChatContainer>
  )
}
