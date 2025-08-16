import { createBrowserRouter, Navigate, RouterProvider } from "react-router-dom"
import App from "./App"
import LoginPage from "./pages/LoginPage"
import { useAuth } from "./contexts/AuthContext"
import ChatRoomPage from "./pages/ChatRoomPage"

export const ApplicationRouterProvider = () => {
  const { isAuthenticated } = useAuth()

  const router = createBrowserRouter([
    {
      path: "/",
      element: <App />,
      children: [
        {
          index: true,
          element: <Navigate to={isAuthenticated ? "/chat" : "/login"} replace />
        },
        {
          path: "/login",
          element: isAuthenticated ? <Navigate to="/chat" replace /> : <LoginPage />
        },
        {
          path: "/chat",
          element: <ChatRoomPage />
        }
      ]
    }
  ])

  return <RouterProvider router={router} />
}
