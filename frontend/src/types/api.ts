// API request/response types and generic API interfaces

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

export interface ApiError {
  message: string
}
