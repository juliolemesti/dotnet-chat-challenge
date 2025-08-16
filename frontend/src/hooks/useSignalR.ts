import { useState, useEffect, useCallback, useRef } from 'react'
import signalRService from '../services/signalRService'
import { SignalRCallbacks } from '../services/signalRService'
import { SignalRMessageDto, SignalRRoomDto, SignalRUserPresenceDto, SignalRErrorDto } from '../types/signalr'

interface UseSignalRReturn {
  isConnected: boolean
  isConnecting: boolean
  error: string | null
  hasReachedMaxRetries: boolean
  sendMessage: (roomId: number, message: string) => Promise<void>
  joinRoom: (roomId: number) => Promise<void>
  leaveRoom: (roomId: number) => Promise<void>
  connect: () => Promise<void>
  disconnect: () => Promise<void>
  retryConnection: () => Promise<void>
  clearError: () => void
}

export interface UseSignalROptions {
  onMessageReceived?: (message: SignalRMessageDto) => void
  onUserJoined?: (presence: SignalRUserPresenceDto) => void
  onUserLeft?: (presence: SignalRUserPresenceDto) => void
  onRoomCreated?: (room: SignalRRoomDto) => void
  onJoinedRoom?: (roomId: string) => void
  onLeftRoom?: (roomId: string) => void
  onError?: (error: SignalRErrorDto) => void
  autoConnect?: boolean
}

export const useSignalR = (options: UseSignalROptions = {}): UseSignalRReturn => {
  const [isConnected, setIsConnected] = useState(false)
  const [isConnecting, setIsConnecting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [hasReachedMaxRetries, setHasReachedMaxRetries] = useState(false)
  const [hasAttemptedAutoConnect, setHasAttemptedAutoConnect] = useState(false)
  const hasInitialized = useRef(false)

  const {
    onMessageReceived,
    onUserJoined,
    onUserLeft,
    onRoomCreated,
    onJoinedRoom,
    onLeftRoom,
    onError,
    autoConnect = true
  } = options

  // Setup SignalR callbacks
  useEffect(() => {
    const callbacks: SignalRCallbacks = {
      onConnected: () => {
        console.log('SignalR connected successfully')
        setIsConnected(true)
        setIsConnecting(false)
        setError(null)
        setHasReachedMaxRetries(false)
      },
      onDisconnected: (err) => {
        console.log('SignalR disconnected')
        setIsConnected(false)
        setIsConnecting(false)
        if (err) {
          setError(err.message || 'Connection lost')
        }
      },
      onMessageReceived: onMessageReceived,
      onUserJoined: onUserJoined,
      onUserLeft: onUserLeft,
      onRoomCreated: onRoomCreated,
      onJoinedRoom: onJoinedRoom,
      onLeftRoom: onLeftRoom,
      onError: (err) => {
        console.error('SignalR Error:', err)
        setError(err.message)
        onError?.(err)
      },
      onAuthenticationFailed: () => {
        console.warn('SignalR authentication failed')
        setError('Authentication failed')
        setIsConnected(false)
        setIsConnecting(false)
        // Redirect handled by service
      },
      onMaxRetriesReached: () => {
        console.warn('SignalR max retries reached')
        setHasReachedMaxRetries(true)
        setIsConnecting(false)
        setError('Connection failed after maximum retries. Click retry to try again.')
      }
    }

    signalRService.setCallbacks(callbacks)
  }, [onMessageReceived, onUserJoined, onUserLeft, onRoomCreated, onJoinedRoom, onLeftRoom, onError])

  // Auto-connect on mount - only once
  useEffect(() => {
    if (autoConnect && !hasInitialized.current && !hasAttemptedAutoConnect && !isConnected && !isConnecting) {
      console.log('Attempting auto-connect...')
      hasInitialized.current = true
      setHasAttemptedAutoConnect(true)
      setIsConnecting(true)
      signalRService.startConnection().catch((err) => {
        console.error('Failed to start SignalR connection:', err)
        setError(err.message || 'Failed to connect')
        setIsConnecting(false)
      })
    }

    // Cleanup on unmount
    return () => {
      signalRService.stopConnection().catch(console.error)
    }
  }, []) // Empty dependency array to run only once

  const sendMessage = useCallback(async (roomId: number, message: string): Promise<void> => {
    try {
      setError(null)
      await signalRService.sendMessage(roomId, message)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to send message'
      setError(errorMessage)
      throw err
    }
  }, [])

  const joinRoom = useCallback(async (roomId: number): Promise<void> => {
    try {
      setError(null)
      await signalRService.joinRoom(roomId)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to join room'
      setError(errorMessage)
      throw err
    }
  }, [])

  const leaveRoom = useCallback(async (roomId: number): Promise<void> => {
    try {
      setError(null)
      await signalRService.leaveRoom(roomId)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to leave room'
      setError(errorMessage)
      throw err
    }
  }, [])

  const clearError = useCallback(() => {
    setError(null)
  }, [])

  const connect = useCallback(async (): Promise<void> => {
    try {
      setIsConnecting(true)
      setError(null)
      setHasReachedMaxRetries(false)
      setHasAttemptedAutoConnect(true)
      await signalRService.startConnection()
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to connect'
      setError(errorMessage)
      setIsConnecting(false)
      throw err
    }
  }, [])

  const disconnect = useCallback(async (): Promise<void> => {
    try {
      setError(null)
      await signalRService.stopConnection()
      setIsConnected(false)
      setIsConnecting(false)
      setHasReachedMaxRetries(false)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to disconnect'
      setError(errorMessage)
      throw err
    }
  }, [])

  const retryConnection = useCallback(async (): Promise<void> => {
    try {
      setIsConnecting(true)
      setError(null)
      setHasReachedMaxRetries(false)
      setHasAttemptedAutoConnect(true)
      await signalRService.retryConnection()
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to retry connection'
      setError(errorMessage)
      setIsConnecting(false)
      throw err
    }
  }, [])

  return {
    isConnected,
    isConnecting,
    error,
    hasReachedMaxRetries,
    sendMessage,
    joinRoom,
    leaveRoom,
    connect,
    disconnect,
    retryConnection,
    clearError
  }
}
