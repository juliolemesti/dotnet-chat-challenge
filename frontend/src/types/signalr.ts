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
