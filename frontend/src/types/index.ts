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

export type AuthMode = 'login' | 'register'
