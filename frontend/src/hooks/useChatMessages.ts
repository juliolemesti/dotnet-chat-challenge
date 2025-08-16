import { useState, useEffect, useCallback } from 'react'
import { chatApi } from '../services/chatService'
import { ChatMessage, SendMessageRequest } from '../types'
import { SignalRMessageDto } from '../types/signalr'
import { SignalRService } from '../services/signalRService'

interface UseChatMessagesReturn {
  messages: ChatMessage[]
  isLoading: boolean
  isSending: boolean
  error: string | null
  sendMessage: (content: string) => Promise<void>
  sendMessageViaSignalR: (content: string) => Promise<void>
  refreshMessages: () => Promise<void>
  clearMessages: () => void
  clearError: () => void
  handleMessageReceived: (messageDto: SignalRMessageDto) => void
}

interface UseChatMessagesOptions {
  roomId: number | null
  autoFetch?: boolean
  maxMessages?: number
}

export const useChatMessages = (options: UseChatMessagesOptions): UseChatMessagesReturn => {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [isSending, setIsSending] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { roomId, autoFetch = true, maxMessages = 50 } = options

  const refreshMessages = useCallback(async (): Promise<void> => {
    if (!roomId) {
      setMessages([])
      return
    }

    try {
      setIsLoading(true)
      setError(null)
      
      const fetchedMessages = await chatApi.getMessages(roomId)
      
      // Sort messages by creation date (oldest first)
      const sortedMessages = fetchedMessages.sort((a, b) => 
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      )
      
      // Limit to maxMessages (keep most recent)
      const limitedMessages = sortedMessages.slice(-maxMessages)
      
      setMessages(limitedMessages)
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to fetch messages'
      setError(errorMessage)
      console.error('Error fetching messages:', err)
    } finally {
      setIsLoading(false)
    }
  }, [roomId, maxMessages])

  const sendMessage = useCallback(async (content: string): Promise<void> => {
    if (!roomId || !content.trim()) {
      throw new Error('Room ID and message content are required')
    }

    try {
      setIsSending(true)
      setError(null)
      
      const messageData: SendMessageRequest = { content: content.trim() }
      const sentMessage = await chatApi.sendMessage(roomId, messageData)
      
      // Add message to local state (in case SignalR doesn't broadcast it back)
      setMessages(prev => {
        const exists = prev.some(msg => msg.id === sentMessage.id)
        if (exists) return prev
        
        const newMessages = [...prev, sentMessage].slice(-maxMessages)
        return newMessages
      })
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to send message'
      setError(errorMessage)
      throw err
    } finally {
      setIsSending(false)
    }
  }, [roomId, maxMessages])

  const sendMessageViaSignalR = useCallback(async (content: string): Promise<void> => {
    if (!roomId || !content.trim()) {
      throw new Error('Room ID and message content are required')
    }

    try {
      setIsSending(true)
      setError(null)
      
      // Import signalRService dynamically to avoid circular dependencies
      const signalRService = (await import('../services/signalRService')).default
      await signalRService.sendMessage(roomId, content.trim())
      
      // Don't add message to local state here - wait for SignalR to broadcast it back
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to send message via SignalR'
      setError(errorMessage)
      throw err
    } finally {
      setIsSending(false)
    }
  }, [roomId])

  const handleMessageReceived = useCallback((messageDto: SignalRMessageDto): void => {
    // Only add messages for the current room
    if (!roomId || messageDto.roomId !== roomId) {
      return
    }

    const newMessage: ChatMessage = SignalRService.signalRMessageToChatMessage(messageDto)
    
    setMessages(prev => {
      // Check if message already exists to avoid duplicates
      const exists = prev.some(msg => msg.id === newMessage.id)
      if (exists) return prev
      
      // Add new message and maintain maxMessages limit
      const updatedMessages = [...prev, newMessage]
        .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
        .slice(-maxMessages)
      
      return updatedMessages
    })
  }, [roomId, maxMessages])

  const clearMessages = useCallback(() => {
    setMessages([])
    setError(null)
  }, [])

  const clearError = useCallback(() => {
    setError(null)
  }, [])

  // Auto-fetch messages when roomId changes
  useEffect(() => {
    if (autoFetch && roomId) {
      refreshMessages()
    } else if (!roomId) {
      clearMessages()
    }
  }, [roomId, autoFetch, refreshMessages, clearMessages])

  // Clear messages when roomId becomes null
  useEffect(() => {
    if (!roomId) {
      setMessages([])
    }
  }, [roomId])

  return {
    messages,
    isLoading,
    isSending,
    error,
    sendMessage,
    sendMessageViaSignalR,
    refreshMessages,
    clearMessages,
    clearError,
    handleMessageReceived
  }
}
