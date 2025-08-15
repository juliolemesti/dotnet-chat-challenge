import axios, { AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { AuthResponse, LoginRequest, RegisterRequest } from '../types'

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7042/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Add request interceptor to include auth token
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('authToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const authApi = {
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    try {
      const response: AxiosResponse<AuthResponse> = await api.post('/auth/login', credentials)
      return response.data
    } catch (error: any) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Login failed. Please check your credentials.')
    }
  },

  register: async (userData: RegisterRequest): Promise<AuthResponse> => {
    try {
      const response: AxiosResponse<AuthResponse> = await api.post('/auth/register', userData)
      return response.data
    } catch (error: any) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Registration failed. Please try again.')
    }
  }
}

export default api
