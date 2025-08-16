// UI and state management types
import { ChatRoom, ChatMessage } from './chat'

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
