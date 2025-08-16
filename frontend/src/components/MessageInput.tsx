import React from 'react'
import {
  Box,
  TextField,
  IconButton,
  Paper,
  Chip,
  Tooltip
} from '@mui/material'
import {
  Send as SendIcon,
  ConnectWithoutContact as DisconnectedIcon
} from '@mui/icons-material'
import { styled } from '@mui/material/styles'
import { useMessageInput } from '../hooks'

interface MessageInputProps {
  onSendMessage: (message: string) => Promise<void>
  disabled?: boolean
  isConnected: boolean
  placeholder?: string
}

const InputContainer = styled(Paper)(({ theme }) => ({
  padding: theme.spacing(1, 2),
  backgroundColor: theme.palette.background.paper,
  borderTop: `1px solid ${theme.palette.divider}`,
  [theme.breakpoints.down('sm')]: {
    padding: theme.spacing(1),
  }
}))

const InputWrapper = styled(Box)(({ theme }) => ({
  flex: 1,
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing(0.5)
}))

const StatusChip = styled(Chip, {
  shouldForwardProp: (prop) => prop !== 'connected'
})<{ connected: boolean }>(({ theme, connected }) => ({
  fontSize: '0.75rem',
  height: 24,
  backgroundColor: connected ? theme.palette.success.light : theme.palette.error.light,
  color: connected ? theme.palette.success.contrastText : theme.palette.error.contrastText,
  '& .MuiChip-icon': {
    color: 'inherit'
  }
}))

const StyledTextField = styled(TextField)(({ theme }) => ({
  '& .MuiOutlinedInput-root': {
    borderRadius: theme.spacing(3),
    backgroundColor: theme.palette.action.hover,
    '&:hover': {
      backgroundColor: theme.palette.action.selected
    },
    '&.Mui-focused': {
      backgroundColor: theme.palette.background.paper
    }
  },
  [theme.breakpoints.down('sm')]: {
    '& .MuiOutlinedInput-root': {
      borderRadius: theme.spacing(2),
      fontSize: '1rem'
    }
  }
}))

export const MessageInput: React.FC<MessageInputProps> = ({
  onSendMessage,
  disabled = false,
  isConnected,
  placeholder = "Type your message... (Try /stock=AAPL.US for stock quotes)"
}) => {
  const { value, setValue, handleSubmit, handleKeyPress, isSubmitting } = useMessageInput()

  const handleSend = async (): Promise<void> => {
    await handleSubmit(onSendMessage)
  }

  const isDisabled = disabled || !isConnected || isSubmitting

  return (
    <InputContainer elevation={2}>
      <InputWrapper>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <StatusChip
            connected={isConnected}
            size="small"
            icon={isConnected ? undefined : <DisconnectedIcon />}
            label={isConnected ? 'Connected' : 'Disconnected'}
            variant="filled"
          />
        </Box>
        
        <StyledTextField
          fullWidth
          multiline
          maxRows={4}
          value={value}
          onChange={(e) => setValue(e.target.value)}
          onKeyPress={handleKeyPress(onSendMessage)}
          placeholder={placeholder}
          disabled={isDisabled}
          variant="outlined"
          size="small"
          helperText={
            !isConnected 
              ? "Reconnecting..." 
              : isSubmitting 
                ? "Sending..." 
                : "Press Enter to send, Shift+Enter for new line"
          }
          FormHelperTextProps={{
            sx: { fontSize: '0.7rem', mx: 0 }
          }}
        />
      </InputWrapper>

      <Tooltip title={isDisabled ? "Cannot send message" : "Send message"}>
        <span>
          <IconButton
            color="primary"
            onClick={handleSend}
            disabled={isDisabled || !value.trim()}
            size="large"
            sx={{
              bgcolor: 'primary.main',
              color: 'primary.contrastText',
              '&:hover': {
                bgcolor: 'primary.dark'
              },
              '&.Mui-disabled': {
                bgcolor: 'action.disabledBackground',
                color: 'action.disabled'
              }
            }}
          >
            <SendIcon />
          </IconButton>
        </span>
      </Tooltip>
    </InputContainer>
  )
}
