import React from 'react'
import {
  Box,
  Paper,
  Typography,
  Avatar,
  Chip
} from '@mui/material'
import { styled } from '@mui/material/styles'
import { ChatMessage } from '../types'

interface MessageItemProps {
  message: ChatMessage
  isCurrentUser?: boolean
}

const MessageBubble = styled(Paper, {
  shouldForwardProp: (prop) => prop !== 'isCurrentUser'
})<{ isCurrentUser: boolean }>(({ theme, isCurrentUser }) => ({
  padding: theme.spacing(1, 1.5),
  maxWidth: '70%',
  backgroundColor: isCurrentUser 
    ? theme.palette.primary.main 
    : theme.palette.background.paper,
  color: isCurrentUser 
    ? theme.palette.primary.contrastText 
    : theme.palette.text.primary,
  borderRadius: isCurrentUser 
    ? '18px 4px 18px 18px'
    : '18px 18px 18px 4px',
  boxShadow: theme.shadows[1],
  [theme.breakpoints.down('sm')]: {
    maxWidth: '85%',
    padding: theme.spacing(0.75, 1.25)
  }
}))

const UserInfo = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  gap: theme.spacing(1),
  marginBottom: theme.spacing(0.5)
}))

const MessageContent = styled(Typography)(({ theme }) => ({
  wordBreak: 'break-word',
  whiteSpace: 'pre-wrap'
}))

const TimeStamp = styled(Typography)(({ theme }) => ({
  fontSize: '0.75rem',
  opacity: 0.7,
  textAlign: 'right',
  marginTop: theme.spacing(0.5)
}))

export const MessageItem: React.FC<MessageItemProps> = ({ message, isCurrentUser = false }) => {
  const formatTime = (dateString: string): string => {
    const date = new Date(dateString)
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  }

  const getUserInitials = (userName: string): string => {
    return userName
      .split(' ')
      .map(name => name.charAt(0))
      .join('')
      .toUpperCase()
      .slice(0, 2)
  }

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: isCurrentUser ? 'flex-end' : 'flex-start',
        mb: 1
      }}
    >
      <MessageBubble isCurrentUser={isCurrentUser} elevation={1}>
        {!isCurrentUser && (
          <UserInfo>
            <Avatar
              sx={{
                width: 24,
                height: 24,
                fontSize: '0.75rem',
                bgcolor: message.isStockBot ? 'warning.main' : 'secondary.main'
              }}
            >
              {message.isStockBot ? '🤖' : getUserInitials(message.userName)}
            </Avatar>
            <Typography variant="caption" fontWeight="medium">
              {message.userName}
            </Typography>
            {message.isStockBot && (
              <Chip
                label="Stock Bot"
                size="small"
                color="warning"
                variant="outlined"
                sx={{ height: 20, fontSize: '0.625rem' }}
              />
            )}
          </UserInfo>
        )}
        
        <MessageContent variant="body2">
          {message.content}
        </MessageContent>
        
        <TimeStamp variant="caption">
          {formatTime(message.createdAt)}
        </TimeStamp>
      </MessageBubble>
    </Box>
  )
}
