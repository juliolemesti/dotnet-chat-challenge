import { useState, useEffect, useCallback, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import signalRService from '../services/signalRService'
import { SignalRCallbacks } from '../services/signalRService'
import { SignalRMessageDto, SignalRRoomDto, SignalRUserPresenceDto, SignalRErrorDto } from '../types/signalr'
import { useAuth } from '../contexts/AuthContext'

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
  const hasAttemptedAutoConnectRef = useRef(false)
  const { isAuthenticated } = useAuth()

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

  // Auto-connect only once when auth is ready and user is authenticated
  useEffect(() => {
    if (!autoConnect || !isAuthenticated || hasAttemptedAutoConnectRef.current) return

    // Check if connection is already connected or connecting
    const connectionState = signalRService.getConnectionState()
    console.log('Current SignalR connection state:', connectionState, signalR.HubConnectionState[connectionState])
    
    if (connectionState === signalR.HubConnectionState.Connected || 
        connectionState === signalR.HubConnectionState.Connecting || 
        connectionState === signalR.HubConnectionState.Reconnecting) {
      console.log('SignalR already connected/connecting, skipping auto-connect')
      return
    }

    console.log('auto connect effect executed', isAuthenticated, autoConnect, !hasAttemptedAutoConnectRef.current)
    hasAttemptedAutoConnectRef.current = true
    setIsConnecting(true)
    
    console.log('Starting SignalR connection immediately...')
    // Start connection immediately instead of using timeout
    signalRService.startConnection().catch((err) => {
      console.error("Failed to start SignalR connection:", err)
      setError(err.message || "Failed to connect")
      setIsConnecting(false)
      hasAttemptedAutoConnectRef.current = false
    })
  }, [isAuthenticated, autoConnect])

  // Disconnect when user logs out
  useEffect(() => {
    if (!isAuthenticated && isConnected) {
      console.log('User logged out, disconnecting SignalR...')
      signalRService.stopConnection().catch(console.error)
      hasAttemptedAutoConnectRef.current = false
    }
  }, [isAuthenticated, isConnected])

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      // Only disconnect if we initiated the auto-connect
      if (hasAttemptedAutoConnectRef.current && signalRService.isConnected()) {
        console.log('useSignalR cleanup: disconnecting SignalR')
        signalRService.stopConnection().catch(console.error)
      }
    }
  }, [])

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
      console.log('🏠 Attempting to join SignalR room:', roomId)
      await signalRService.joinRoom(roomId)
      console.log('🏠 Successfully joined SignalR room:', roomId)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to join room'
      console.error('🏠 Failed to join SignalR room:', roomId, err)
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
