import { Navigate, useLocation } from 'react-router-dom'

import { useAuth } from '../../auth/AuthContext'

function RequireAuth({ children }) {
  const location = useLocation()
  const { isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return children
}

export { RequireAuth }
