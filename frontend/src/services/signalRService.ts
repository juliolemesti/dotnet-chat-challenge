import * as signalR from '@microsoft/signalr'
import {
  SignalRMessageDto,
  SignalRRoomDto,
  SignalRUserPresenceDto,
  SignalRErrorDto,
  SignalRConnectionDto,
  ChatMessage
} from '../types'
import { HUB_URL } from '../util/consts'
import { handleAuthenticationError, isAuthenticationError, hasValidToken } from '../util/authUtils'

export interface SignalRCallbacks {
  onConnected?: (data: SignalRConnectionDto) => void
  onDisconnected?: (error?: Error) => void
  onMessageReceived?: (message: SignalRMessageDto) => void
  onUserJoined?: (presence: SignalRUserPresenceDto) => void
  onUserLeft?: (presence: SignalRUserPresenceDto) => void
  onRoomCreated?: (room: SignalRRoomDto) => void
  onJoinedRoom?: (roomId: string) => void
  onLeftRoom?: (roomId: string) => void
  onError?: (error: SignalRErrorDto) => void
  onAuthenticationFailed?: () => void
  onMaxRetriesReached?: () => void
}

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private callbacks: SignalRCallbacks = {}
  private reconnectAttempts = 0
  private maxReconnectAttempts = 3 // Reduced from 5 to 3
  private reconnectDelay = 3000
  private hasReachedMaxRetries = false
  private userInitiatedReconnect = false

  constructor() {
    this.setupConnection()
  }

  private setupConnection(): void {
    // Check if token exists before setting up connection
    if (!hasValidToken()) {
      console.warn('No authentication token found')
      handleAuthenticationError()
      return
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => {
          const currentToken = localStorage.getItem('authToken')
          if (!currentToken) {
            console.warn('Token not found during connection attempt')
            handleAuthenticationError()
            return ''
          }
          return currentToken
        },
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Don't retry if it's an authentication error
          if (isAuthenticationError(retryContext.retryReason)) {
            console.warn('Authentication error - stopping reconnect attempts')
            handleAuthenticationError()
            return null
          }
          
          if (retryContext.previousRetryCount < this.maxReconnectAttempts && !this.hasReachedMaxRetries) {
            return this.reconnectDelay
          }
          
          // Max retries reached for automatic reconnection
          if (!this.hasReachedMaxRetries) {
            this.hasReachedMaxRetries = true
            console.warn(`Max retry attempts (${this.maxReconnectAttempts}) reached. Manual reconnection required.`)
            this.callbacks.onMaxRetriesReached?.()
          }
          
          return null // Stop automatic retrying
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build()

    this.setupEventHandlers()
  }

  private setupEventHandlers(): void {
    if (!this.connection) return

    // Connection events
    this.connection.onclose((error) => {
      console.log('SignalR connection closed', error)
      
      // Check if this is an authentication error
      if (isAuthenticationError(error)) {
        console.warn('Connection closed due to authentication error')
        this.callbacks.onAuthenticationFailed?.()
        handleAuthenticationError()
        return
      }
      
      this.callbacks.onDisconnected?.(error || undefined)
    })

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected')
      this.reconnectAttempts = 0
      this.hasReachedMaxRetries = false
      this.userInitiatedReconnect = false
    })

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error)
      this.reconnectAttempts++
    })

    // Server to client events
    this.connection.on('Connected', (data: SignalRConnectionDto) => {
      console.log('SignalR Connected:', data)
      this.callbacks.onConnected?.(data)
    })

    this.connection.on('ReceiveMessage', (message: SignalRMessageDto) => {
      console.log('Message received:', message)
      this.callbacks.onMessageReceived?.(message)
    })

    this.connection.on('UserJoined', (presence: SignalRUserPresenceDto) => {
      console.log('User joined:', presence)
      this.callbacks.onUserJoined?.(presence)
    })

    this.connection.on('UserLeft', (presence: SignalRUserPresenceDto) => {
      console.log('User left:', presence)
      this.callbacks.onUserLeft?.(presence)
    })

    this.connection.on('RoomCreated', (room: SignalRRoomDto) => {
      console.log('Room created:', room)
      this.callbacks.onRoomCreated?.(room)
    })

    this.connection.on('JoinedRoom', (roomId: string) => {
      console.log('Joined room:', roomId)
      this.callbacks.onJoinedRoom?.(roomId)
    })

    this.connection.on('LeftRoom', (roomId: string) => {
      console.log('Left room:', roomId)
      this.callbacks.onLeftRoom?.(roomId)
    })

    this.connection.on('Error', (error: SignalRErrorDto) => {
      console.error('SignalR Error:', error)
      
      // Check if this is an authentication error
      if (error.code === 'AUTH_REQUIRED' || error.code === 'UNAUTHORIZED' || 
          error.message?.toLowerCase().includes('authentication') ||
          error.message?.toLowerCase().includes('unauthorized')) {
        console.warn('SignalR authentication error received')
        this.callbacks.onAuthenticationFailed?.()
        handleAuthenticationError()
        return
      }
      
      this.callbacks.onError?.(error)
    })
  }

  async startConnection(): Promise<void> {
    // Check if token exists before attempting connection
    if (!hasValidToken()) {
      console.warn('No authentication token found - cannot start connection')
      handleAuthenticationError()
      throw new Error('Authentication required')
    }

    if (!this.connection) {
      this.setupConnection()
    }

    if (this.connection?.state === signalR.HubConnectionState.Disconnected) {
      try {
        await this.connection.start()
        console.log('SignalR connection started')
        this.reconnectAttempts = 0
        this.hasReachedMaxRetries = false
        this.userInitiatedReconnect = false
      } catch (error: any) {
        console.error('SignalR connection failed:', error)
        
        // Check if this is an authentication error
        if (isAuthenticationError(error)) {
          console.warn('Connection failed due to authentication error')
          this.callbacks.onAuthenticationFailed?.()
          handleAuthenticationError()
          throw new Error('Authentication failed')
        }
        
        throw error
      }
    }
  }

  async retryConnection(): Promise<void> {
    console.log('User initiated reconnection attempt')
    this.userInitiatedReconnect = true
    this.hasReachedMaxRetries = false
    this.reconnectAttempts = 0
    
    // Stop existing connection if any
    if (this.connection && this.connection.state !== signalR.HubConnectionState.Disconnected) {
      await this.stopConnection()
    }
    
    // Setup new connection and start
    this.setupConnection()
    await this.startConnection()
  }

  async stopConnection(): Promise<void> {
    if (this.connection && this.connection.state !== signalR.HubConnectionState.Disconnected) {
      try {
        await this.connection.stop()
        console.log('SignalR connection stopped')
      } catch (error) {
        console.error('Error stopping SignalR connection:', error)
        throw error
      }
    }
  }

  async sendMessage(roomId: number, message: string): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection not established')
    }

    try {
      await this.connection.invoke('SendMessage', roomId.toString(), message)
    } catch (error: any) {
      console.error('Error sending message:', error)
      
      // Check if this is an authentication error
      if (isAuthenticationError(error)) {
        console.warn('Send message failed due to authentication error')
        this.callbacks.onAuthenticationFailed?.()
        handleAuthenticationError()
        throw new Error('Authentication failed')
      }
      
      throw error
    }
  }

  async joinRoom(roomId: number): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection not established')
    }

    try {
      await this.connection.invoke('JoinRoom', roomId.toString())
    } catch (error: any) {
      console.error('Error joining room:', error)
      
      // Check if this is an authentication error
      if (isAuthenticationError(error)) {
        console.warn('Join room failed due to authentication error')
        this.callbacks.onAuthenticationFailed?.()
        handleAuthenticationError()
        throw new Error('Authentication failed')
      }
      
      throw error
    }
  }

  async leaveRoom(roomId: number): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection not established')
    }

    try {
      await this.connection.invoke('LeaveRoom', roomId.toString())
    } catch (error: any) {
      console.error('Error leaving room:', error)
      
      // Check if this is an authentication error
      if (isAuthenticationError(error)) {
        console.warn('Leave room failed due to authentication error')
        this.callbacks.onAuthenticationFailed?.()
        handleAuthenticationError()
        throw new Error('Authentication failed')
      }
      
      throw error
    }
  }

  setCallbacks(callbacks: SignalRCallbacks): void {
    this.callbacks = { ...this.callbacks, ...callbacks }
  }

  getConnectionState(): signalR.HubConnectionState {
    return this.connection?.state || signalR.HubConnectionState.Disconnected
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }

  hasReachedMaxReconnectAttempts(): boolean {
    return this.hasReachedMaxRetries
  }

  getReconnectAttempts(): number {
    return this.reconnectAttempts
  }

  getMaxReconnectAttempts(): number {
    return this.maxReconnectAttempts
  }

  // Utility method to convert SignalR DTO to ChatMessage
  static signalRMessageToChatMessage(signalRMessage: SignalRMessageDto): ChatMessage {
    return {
      id: signalRMessage.id,
      content: signalRMessage.content,
      userName: signalRMessage.userName,
      chatRoomId: signalRMessage.roomId,
      createdAt: signalRMessage.createdAt,
      isStockBot: signalRMessage.isStockBot
    }
  }
}

// Create singleton instance
const signalRService = new SignalRService()

export default signalRService
export { SignalRService }
