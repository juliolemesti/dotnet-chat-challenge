import axios, { AxiosResponse } from 'axios'
import {
  ChatMessage,
  ChatRoom,
  CreateRoomRequest,
  SendMessageRequest
} from '../types'
import { handleAuthenticationError, isAuthenticationError } from '../util/authUtils'
import { API_BASE_URL } from '../util/consts'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Add request interceptor to include auth token and handle missing tokens
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken')
    if (!token) {
      console.warn('No authentication token found - redirecting to login')
      handleAuthenticationError()
      return Promise.reject(new Error('Authentication required'))
    }
    config.headers.Authorization = `Bearer ${token}`
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Add response interceptor to handle authentication errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle authentication errors globally
    if (isAuthenticationError(error)) {
      console.warn('API authentication error:', error)
      handleAuthenticationError()
    }
    return Promise.reject(error)
  }
)

export const chatApi = {
  // Get all chat rooms
  getRooms: async (): Promise<ChatRoom[]> => {
    try {
      const response: AxiosResponse<ChatRoom[]> = await api.get('/chat/rooms')
      return response.data
    } catch (error: any) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Failed to fetch rooms')
    }
  },

  // Create a new chat room
  createRoom: async (roomData: CreateRoomRequest): Promise<ChatRoom> => {
    try {
      const response: AxiosResponse<ChatRoom> = await api.post('/chat/rooms', roomData)
      return response.data
    } catch (error: any) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Failed to create room')
    }
  },

  // Get messages for a specific room (last 50 messages)
  getMessages: async (roomId: number): Promise<ChatMessage[]> => {
    try {
      const response: AxiosResponse<ChatMessage[]> = await api.get(`/chat/rooms/${roomId}/messages`)
      return response.data
    } catch (error: any) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Failed to fetch messages')
    }
  },

  // Send a message via REST API (fallback, mainly using SignalR)
  sendMessage: async (roomId: number, messageData: SendMessageRequest): Promise<ChatMessage> => {
    try {
      const response: AxiosResponse<ChatMessage> = await api.post(`/chat/rooms/${roomId}/messages`, messageData)
      return response.data
    } catch (error: any) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Failed to send message')
    }
  }
}

export default chatApi
