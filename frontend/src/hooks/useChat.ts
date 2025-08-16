import { useCallback } from 'react'
import { useSignalR } from './useSignalR'
import { useChatRooms } from './useChatRooms'
import { useChatMessages } from './useChatMessages'
import { CreateRoomRequest } from '../types'

interface UseChatReturn {
  // SignalR connection state
  isConnected: boolean
  isConnecting: boolean
  connectionError: string | null
  
  // Rooms state
  rooms: ReturnType<typeof useChatRooms>['rooms']
  selectedRoomId: ReturnType<typeof useChatRooms>['selectedRoomId']
  isLoadingRooms: boolean
  roomsError: string | null
  
  // Messages state
  messages: ReturnType<typeof useChatMessages>['messages']
  isLoadingMessages: boolean
  isSendingMessage: boolean
  messagesError: string | null
  
  // Actions
  selectRoom: (roomId: number | null) => void
  createRoom: (roomData: CreateRoomRequest) => Promise<void>
  sendMessage: (content: string) => Promise<void>
  joinRoom: (roomId: number) => Promise<void>
  leaveRoom: (roomId: number) => Promise<void>
  refreshRooms: () => Promise<void>
  refreshMessages: () => Promise<void>
  
  // Error handling
  clearAllErrors: () => void
}

export const useChat = (): UseChatReturn => {
  // Initialize rooms hook first
  const {
    rooms,
    selectedRoomId,
    isLoading: isLoadingRooms,
    error: roomsError,
    refreshRooms,
    createRoom: createRoomAction,
    selectRoom: selectRoomAction,
    clearError: clearRoomsError,
    handleRoomCreatedFromSignalR
  } = useChatRooms()

  // Initialize messages hook with selected room
  const {
    messages,
    isLoading: isLoadingMessages,
    isSending: isSendingMessage,
    error: messagesError,
    sendMessageViaSignalR,
    refreshMessages,
    clearError: clearMessagesError,
    handleMessageReceived
  } = useChatMessages({ roomId: selectedRoomId })

  // Initialize SignalR hook with callbacks
  const {
    isConnected,
    isConnecting,
    error: connectionError,
    joinRoom: joinRoomAction,
    leaveRoom: leaveRoomAction,
    clearError: clearConnectionError
  } = useSignalR({
    onMessageReceived: handleMessageReceived,
    onRoomCreated: handleRoomCreatedFromSignalR,
    autoConnect: true
  })

  // Enhanced room selection with automatic SignalR room joining
  const selectRoom = useCallback(async (roomId: number | null): Promise<void> => {
    try {
      // Leave current room if connected and a room is selected
      if (isConnected && selectedRoomId) {
        await leaveRoomAction(selectedRoomId)
      }
      
      // Select new room
      selectRoomAction(roomId)
      
      // Join new room if connected and room is selected
      if (isConnected && roomId) {
        await joinRoomAction(roomId)
      }
    } catch (error) {
      console.error('Error switching rooms:', error)
      // Still select the room even if SignalR operations fail
      selectRoomAction(roomId)
    }
  }, [isConnected, selectedRoomId, selectRoomAction, joinRoomAction, leaveRoomAction])

  // Enhanced room creation
  const createRoom = useCallback(async (roomData: CreateRoomRequest): Promise<void> => {
    try {
      const newRoom = await createRoomAction(roomData)
      
      // Auto-select and join the newly created room
      if (isConnected) {
        await joinRoomAction(newRoom.id)
      }
    } catch (error) {
      console.error('Error creating room:', error)
      throw error
    }
  }, [createRoomAction, joinRoomAction, isConnected])

  // Send message (prefer SignalR, fallback to REST API)
  const sendMessage = useCallback(async (content: string): Promise<void> => {
    if (!selectedRoomId || !content.trim()) {
      throw new Error('Room must be selected and message cannot be empty')
    }

    try {
      // Use SignalR if connected, otherwise fallback to REST API
      await sendMessageViaSignalR(content.trim())
    } catch (error) {
      console.error('Error sending message:', error)
      throw error
    }
  }, [selectedRoomId, sendMessageViaSignalR])

  // Auto-join selected room when SignalR connects
  const joinRoom = useCallback(async (roomId: number): Promise<void> => {
    await joinRoomAction(roomId)
  }, [joinRoomAction])

  const leaveRoom = useCallback(async (roomId: number): Promise<void> => {
    await leaveRoomAction(roomId)
  }, [leaveRoomAction])

  // Clear all errors
  const clearAllErrors = useCallback(() => {
    clearConnectionError()
    clearRoomsError()
    clearMessagesError()
  }, [clearConnectionError, clearRoomsError, clearMessagesError])

  return {
    // SignalR connection state
    isConnected,
    isConnecting,
    connectionError,
    
    // Rooms state
    rooms,
    selectedRoomId,
    isLoadingRooms,
    roomsError,
    
    // Messages state
    messages,
    isLoadingMessages,
    isSendingMessage,
    messagesError,
    
    // Actions
    selectRoom,
    createRoom,
    sendMessage,
    joinRoom,
    leaveRoom,
    refreshRooms,
    refreshMessages,
    
    // Error handling
    clearAllErrors
  }
}
