// Chat-related types and interfaces

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

export type MessageType = 'user' | 'bot' | 'system'
