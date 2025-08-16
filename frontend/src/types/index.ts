export interface User {
  id: number
  email: string
  userName: string
  createdAt: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  userName: string
  password: string
}

export interface AuthResponse {
  success: boolean
  user: User
  token: string
}

export interface ApiError {
  message: string
}

export interface ChatRoom {
  id: number
  name: string
  createdAt: string
}

export interface ChatMessage {
  id: number
  content: string
  userName: string
  chatRoomId: number
  createdAt: string
  isStockBot: boolean
}

// SignalR DTOs to match backend models
export interface SignalRMessageDto {
  id: number
  content: string
  userName: string
  roomId: number
  createdAt: string
  isStockBot: boolean
}

export interface SignalRRoomDto {
  id: number
  name: string
  createdAt: string
  memberCount: number
}

export interface SignalRUserPresenceDto {
  userName: string
  roomId: string
}

export interface SignalRErrorDto {
  message: string
  code: string
}

export interface SignalRConnectionDto {
  userName: string
  message: string
}

// API Request/Response types
export interface CreateRoomRequest {
  name: string
}

export interface SendMessageRequest {
  content: string
}

export interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
}

// UI State Management types
export interface ChatRoomState {
  selectedRoomId: number | null
  rooms: ChatRoom[]
  messages: ChatMessage[]
  isConnected: boolean
  isLoading: boolean
  error: string | null
}

export interface MessageInputState {
  value: string
  isSubmitting: boolean
}

export type MessageType = 'user' | 'bot' | 'system'

export type AuthMode = 'login' | 'register'
