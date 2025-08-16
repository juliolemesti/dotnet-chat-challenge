// Shared authentication utilities

// Helper function to handle authentication failures and redirect to login
export const handleAuthenticationError = (): void => {
  console.warn('Authentication failed - redirecting to login')
  
  // Clear invalid token from localStorage
  localStorage.removeItem('authToken')
  
  // Redirect to login page if not already there
  if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
    window.location.href = '/login'
  }
}

// Helper function to check if an error is authentication-related
export const isAuthenticationError = (error: any): boolean => {
  if (!error) return false
  
  // Check for HTTP 401/403 status codes
  if (error.statusCode === 401 || error.status === 401 || 
      error.statusCode === 403 || error.status === 403 ||
      error.response?.status === 401 || error.response?.status === 403) {
    return true
  }
  
  // Check error message for authentication keywords
  const errorMessage = error.message?.toLowerCase() || error.toString?.()?.toLowerCase() || ''
  return errorMessage.includes('unauthorized') || 
         errorMessage.includes('401') || 
         errorMessage.includes('403') ||
         errorMessage.includes('authentication') ||
         errorMessage.includes('invalid token') ||
         errorMessage.includes('token expired')
}

// Helper function to check if token exists
export const hasValidToken = (): boolean => {
  const token = localStorage.getItem('authToken')
  return !!token
}
