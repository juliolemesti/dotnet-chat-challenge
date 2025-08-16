import { useState, useEffect, useCallback } from 'react'
import { chatApi } from '../services/chatService'
import { ChatRoom, CreateRoomRequest } from '../types'
import { SignalRRoomDto } from '../types/signalr'

interface UseChatRoomsReturn {
  rooms: ChatRoom[]
  selectedRoomId: number | null
  isLoading: boolean
  error: string | null
  refreshRooms: () => Promise<void>
  createRoom: (roomData: CreateRoomRequest) => Promise<ChatRoom>
  selectRoom: (roomId: number | null) => void
  clearError: () => void
  handleRoomCreatedFromSignalR: (roomDto: SignalRRoomDto) => void
}

interface UseChatRoomsOptions {
  autoFetch?: boolean
  onRoomCreated?: (room: ChatRoom) => void
}

export const useChatRooms = (options: UseChatRoomsOptions = {}): UseChatRoomsReturn => {
  const [rooms, setRooms] = useState<ChatRoom[]>([])
  const [selectedRoomId, setSelectedRoomId] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { autoFetch = true, onRoomCreated } = options

  const refreshRooms = useCallback(async (): Promise<void> => {
    try {
      setIsLoading(true)
      setError(null)
      
      const fetchedRooms = await chatApi.getRooms()
      setRooms(fetchedRooms)
      
      // Auto-select first room if no room is selected and rooms exist
      if (!selectedRoomId && fetchedRooms.length > 0) {
        setSelectedRoomId(fetchedRooms[0].id)
      }
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to fetch rooms'
      setError(errorMessage)
      console.error('Error fetching rooms:', err)
    } finally {
      setIsLoading(false)
    }
  }, [selectedRoomId])

  const createRoom = useCallback(async (roomData: CreateRoomRequest): Promise<ChatRoom> => {
    try {
      setError(null)
      
      const newRoom = await chatApi.createRoom(roomData)
      
      // Add the new room to the list
      setRooms(prev => [...prev, newRoom])
      
      // Auto-select the newly created room
      setSelectedRoomId(newRoom.id)
      
      // Call callback if provided
      onRoomCreated?.(newRoom)
      
      return newRoom
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to create room'
      setError(errorMessage)
      throw err
    }
  }, [onRoomCreated])

  const selectRoom = useCallback((roomId: number | null): void => {
    setSelectedRoomId(roomId)
    setError(null)
  }, [])

  const clearError = useCallback(() => {
    setError(null)
  }, [])

  // Handle SignalR room created events
  const handleRoomCreatedFromSignalR = useCallback((roomDto: SignalRRoomDto): void => {
    const newRoom: ChatRoom = {
      id: roomDto.id,
      name: roomDto.name,
      createdAt: roomDto.createdAt
    }
    
    // Check if room already exists to avoid duplicates
    setRooms(prev => {
      const exists = prev.some(room => room.id === newRoom.id)
      if (exists) return prev
      return [...prev, newRoom]
    })
  }, [])

  // Auto-fetch rooms on mount
  useEffect(() => {
    if (autoFetch) {
      refreshRooms()
    }
  }, [autoFetch, refreshRooms])

  return {
    rooms,
    selectedRoomId,
    isLoading,
    error,
    refreshRooms,
    createRoom,
    selectRoom,
    clearError,
    handleRoomCreatedFromSignalR
  }
}
