// Authentication-related types and interfaces

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

export type AuthMode = 'login' | 'register'
