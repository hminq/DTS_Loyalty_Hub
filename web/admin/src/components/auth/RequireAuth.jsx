import { Navigate, useLocation } from 'react-router-dom'

import { storageKeys } from '../../config/storageKeys'

function RequireAuth({ children }) {
  const location = useLocation()
  const hasAccessToken = Boolean(localStorage.getItem(storageKeys.accessToken))

  if (!hasAccessToken) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return children
}

export { RequireAuth }
