import React from "react"
import ReactDOM from "react-dom/client"
import { AuthProvider, useAuth } from "./contexts/AuthContext"
import "./index.css"
import reportWebVitals from "./reportWebVitals"
import { ApplicationRouterProvider } from "./router"
import { Box, CircularProgress, Typography } from "@mui/material"

const AppContent: React.FC = () => {
  const { isLoading } = useAuth()

  if (isLoading) {
    return (
      <Box 
        sx={{ 
          display: 'flex', 
          flexDirection: 'column',
          alignItems: 'center', 
          justifyContent: 'center', 
          minHeight: '100vh',
          gap: 2
        }}
      >
        <CircularProgress />
        <Typography variant="body1" color="text.secondary">
          Loading...
        </Typography>
      </Box>
    )
  }

  return <ApplicationRouterProvider />
}

const root = ReactDOM.createRoot(document.getElementById("root") as HTMLElement)
root.render(
  <React.StrictMode>
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  </React.StrictMode>
)

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals()
