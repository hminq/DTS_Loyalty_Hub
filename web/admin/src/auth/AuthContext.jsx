import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'

import { storageKeys } from '../config/storageKeys'
import { authEvents } from './authEvents'
import { getAccessToken, removeAccessToken, saveAccessToken } from './tokenStorage'

const AuthContext = createContext(null)

function AuthProvider({ children }) {
  const [accessToken, setAccessTokenState] = useState(getAccessToken)

  const setAccessToken = useCallback((nextAccessToken) => {
    if (!nextAccessToken || !saveAccessToken(nextAccessToken)) {
      removeAccessToken()
      setAccessTokenState(null)
      return false
    }

    setAccessTokenState(nextAccessToken)
    return true
  }, [])

  const clearAccessToken = useCallback(() => {
    removeAccessToken()
    setAccessTokenState(null)
  }, [])

  useEffect(() => {
    function handleUnauthorized() {
      clearAccessToken()
    }

    function handleStorage(event) {
      if (event.key === storageKeys.accessToken) {
        setAccessTokenState(event.newValue)
      }
    }

    window.addEventListener(authEvents.unauthorized, handleUnauthorized)
    window.addEventListener('storage', handleStorage)

    return () => {
      window.removeEventListener(authEvents.unauthorized, handleUnauthorized)
      window.removeEventListener('storage', handleStorage)
    }
  }, [clearAccessToken])

  const value = useMemo(() => ({
    accessToken,
    isAuthenticated: Boolean(accessToken),
    setAccessToken,
    clearAccessToken,
  }), [accessToken, clearAccessToken, setAccessToken])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used within AuthProvider.')
  }

  return context
}

export { AuthProvider, useAuth }
