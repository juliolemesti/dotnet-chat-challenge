import React, { useState } from 'react'
import {
  Box,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Typography,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Paper,
  Divider,
  Badge,
  CircularProgress,
  Alert
} from '@mui/material'
import {
  Add as AddIcon,
  Chat as ChatIcon,
  People as PeopleIcon
} from '@mui/icons-material'
import { styled } from '@mui/material/styles'
import { ChatRoom, CreateRoomRequest } from '../types'

interface RoomsListProps {
  rooms: ChatRoom[]
  selectedRoomId: number | null
  onSelectRoom: (roomId: number) => void
  onCreateRoom: (roomData: CreateRoomRequest) => Promise<void>
  isLoading: boolean
  error: string | null
  isConnected: boolean
}

const SidebarContainer = styled(Paper)(({ theme }) => ({
  height: '100%',
  display: 'flex',
  flexDirection: 'column',
  backgroundColor: theme.palette.background.paper,
  borderRight: `1px solid ${theme.palette.divider}`
}))

const SidebarHeader = styled(Box)(({ theme }) => ({
  padding: theme.spacing(2),
  borderBottom: `1px solid ${theme.palette.divider}`,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between'
}))

const RoomsContainer = styled(Box)(({ theme }) => ({
  flex: 1,
  overflowY: 'auto',
  '&::-webkit-scrollbar': {
    width: '6px'
  },
  '&::-webkit-scrollbar-track': {
    background: theme.palette.action.hover
  },
  '&::-webkit-scrollbar-thumb': {
    background: theme.palette.action.disabled,
    borderRadius: '3px'
  }
}))

const RoomItem = styled(ListItem)<{ selected: boolean }>(({ theme, selected }) => ({
  padding: 0,
  '& .MuiListItemButton-root': {
    padding: theme.spacing(1.5),
    borderRadius: theme.spacing(1),
    margin: theme.spacing(0.5, 1),
    backgroundColor: selected ? theme.palette.primary.light : 'transparent',
    color: selected ? theme.palette.primary.contrastText : theme.palette.text.primary,
    '&:hover': {
      backgroundColor: selected 
        ? theme.palette.primary.main 
        : theme.palette.action.hover
    }
  }
}))

const EmptyState = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  padding: theme.spacing(4),
  textAlign: 'center',
  color: theme.palette.text.secondary,
  flex: 1
}))

export const RoomsList: React.FC<RoomsListProps> = ({
  rooms,
  selectedRoomId,
  onSelectRoom,
  onCreateRoom,
  isLoading,
  error,
  isConnected
}) => {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [newRoomName, setNewRoomName] = useState('')
  const [isCreatingRoom, setIsCreatingRoom] = useState(false)
  const [createError, setCreateError] = useState<string | null>(null)

  const handleOpenCreateDialog = () => {
    setIsCreateDialogOpen(true)
    setNewRoomName('')
    setCreateError(null)
  }

  const handleCloseCreateDialog = () => {
    setIsCreateDialogOpen(false)
    setNewRoomName('')
    setCreateError(null)
  }

  const handleCreateRoom = async () => {
    const trimmedName = newRoomName.trim()
    
    if (!trimmedName) {
      setCreateError('Room name is required')
      return
    }

    if (trimmedName.length < 2) {
      setCreateError('Room name must be at least 2 characters')
      return
    }

    try {
      setIsCreatingRoom(true)
      setCreateError(null)
      
      await onCreateRoom({ name: trimmedName })
      handleCloseCreateDialog()
    } catch (err: any) {
      setCreateError(err.message || 'Failed to create room')
    } finally {
      setIsCreatingRoom(false)
    }
  }

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString)
    return date.toLocaleDateString([], { 
      month: 'short', 
      day: 'numeric'
    })
  }

  return (
    <>
      <SidebarContainer elevation={1}>
        <SidebarHeader>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ChatIcon color="primary" />
            <Typography variant="h6" color="primary">
              Rooms
            </Typography>
          </Box>
          <Button
            startIcon={<AddIcon />}
            onClick={handleOpenCreateDialog}
            disabled={!isConnected}
            size="small"
            variant="outlined"
          >
            New
          </Button>
        </SidebarHeader>

        {error && (
          <Box sx={{ p: 1 }}>
            <Alert severity="error" variant="outlined">
              {error}
            </Alert>
          </Box>
        )}

        <RoomsContainer>
          {isLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress size={32} />
            </Box>
          ) : rooms.length === 0 ? (
            <EmptyState>
              <PeopleIcon sx={{ fontSize: 48, mb: 2, opacity: 0.5 }} />
              <Typography variant="body1" gutterBottom>
                No rooms yet
              </Typography>
              <Typography variant="body2">
                Create your first room to start chatting!
              </Typography>
            </EmptyState>
          ) : (
            <List disablePadding>
              {rooms.map((room) => (
                <RoomItem
                  key={room.id}
                  selected={selectedRoomId === room.id}
                >
                  <ListItemButton
                    onClick={() => onSelectRoom(room.id)}
                    disabled={!isConnected}
                  >
                    <ListItemText
                      primary={room.name}
                      secondary={`Created ${formatDate(room.createdAt)}`}
                      primaryTypographyProps={{
                        fontWeight: selectedRoomId === room.id ? 600 : 400
                      }}
                      secondaryTypographyProps={{
                        fontSize: '0.75rem'
                      }}
                    />
                    {selectedRoomId === room.id && (
                      <Badge
                        badgeContent="●"
                        color="primary"
                        sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem' } }}
                      />
                    )}
                  </ListItemButton>
                </RoomItem>
              ))}
            </List>
          )}
        </RoomsContainer>

        <Divider />
        <Box sx={{ p: 2 }}>
          <Typography variant="caption" color="text.secondary" align="center" display="block">
            {isConnected ? '🟢 Connected' : '🔴 Disconnected'}
          </Typography>
        </Box>
      </SidebarContainer>

      {/* Create Room Dialog */}
      <Dialog
        open={isCreateDialogOpen}
        onClose={handleCloseCreateDialog}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Create New Room</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            fullWidth
            label="Room Name"
            value={newRoomName}
            onChange={(e) => setNewRoomName(e.target.value)}
            onKeyPress={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault()
                handleCreateRoom()
              }
            }}
            placeholder="Enter room name (e.g., General, Random, Tech Talk)"
            margin="normal"
            error={!!createError}
            helperText={createError || "Choose a descriptive name for your room"}
            disabled={isCreatingRoom}
          />
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={handleCloseCreateDialog}
            disabled={isCreatingRoom}
          >
            Cancel
          </Button>
          <Button
            onClick={handleCreateRoom}
            variant="contained"
            disabled={isCreatingRoom || !newRoomName.trim()}
            startIcon={isCreatingRoom ? <CircularProgress size={16} /> : <AddIcon />}
          >
            {isCreatingRoom ? 'Creating...' : 'Create Room'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
