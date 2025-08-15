import React, { useState } from 'react'
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  Link,
  Divider,
} from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { authApi } from '../services/authService'
import { AuthMode } from '../types'

const LoginPage: React.FC = () => {
  const [mode, setMode] = useState<AuthMode>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [userName, setUserName] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const { login } = useAuth()

  const resetForm = () => {
    setEmail('')
    setPassword('')
    setUserName('')
    setError('')
  }

  const toggleMode = () => {
    setMode(mode === 'login' ? 'register' : 'login')
    resetForm()
  }

  const validateForm = (): boolean => {
    if (!email || !password) {
      setError('Please fill in all required fields')
      return false
    }

    if (mode === 'register' && !userName) {
      setError('Username is required for registration')
      return false
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters long')
      return false
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(email)) {
      setError('Please enter a valid email address')
      return false
    }

    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')

    if (!validateForm()) {
      return
    }

    setLoading(true)

    try {
      if (mode === 'login') {
        const response = await authApi.login({ email, password })
        login(response.user, response.token)
        navigate('/chat')
      } else {
        const response = await authApi.register({ email, userName, password })
        login(response.user, response.token)
        navigate('/chat')
      }
    } catch (err: any) {
      // Generic error message - don't specify if it's email or username issue
      if (mode === 'login') {
        setError('Invalid credentials. Please try again.')
      } else {
        if (err.message?.includes('already exists')) {
          setError('An account with these credentials already exists. Please try different credentials or sign in instead.')
        } else {
          setError('Registration failed. Please check your information and try again.')
        }
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <Container component="main" maxWidth="xs">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper
          elevation={3}
          sx={{
            padding: 4,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            width: '100%',
          }}
        >
          <Typography component="h1" variant="h4" gutterBottom>
            Chat Challenge
          </Typography>
          <Typography component="h2" variant="h6" color="text.secondary" gutterBottom>
            {mode === 'login' ? 'Sign in to your account' : 'Create a new account'}
          </Typography>
          
          {error && (
            <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
              {error}
            </Alert>
          )}
          
          <Box component="form" onSubmit={handleSubmit} sx={{ mt: 1, width: '100%' }}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              name="email"
              type="email"
              autoComplete="email"
              autoFocus
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={loading}
            />
            
            {mode === 'register' && (
              <TextField
                margin="normal"
                required
                fullWidth
                id="userName"
                label="Username"
                name="userName"
                autoComplete="username"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                disabled={loading}
                helperText="This will be your display name in the chat"
              />
            )}
            
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={loading}
              helperText={mode === 'register' ? 'Must be at least 6 characters long' : ''}
            />
            
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={loading}
            >
              {loading 
                ? (mode === 'login' ? 'Signing In...' : 'Creating Account...')
                : (mode === 'login' ? 'Sign In' : 'Create Account')
              }
            </Button>
            
            <Divider sx={{ my: 2 }} />
            
            <Box textAlign="center">
              <Typography variant="body2" color="text.secondary">
                {mode === 'login' 
                  ? "Don't have an account? "
                  : "Already have an account? "
                }
                <Link
                  component="button"
                  variant="body2"
                  onClick={toggleMode}
                  disabled={loading}
                  sx={{ cursor: 'pointer' }}
                >
                  {mode === 'login' ? 'Create one here' : 'Sign in here'}
                </Link>
              </Typography>
            </Box>
          </Box>
        </Paper>
      </Box>
    </Container>
  )
}

export default LoginPage
