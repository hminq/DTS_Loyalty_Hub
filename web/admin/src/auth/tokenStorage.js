import { storageKeys } from '../config/storageKeys'

function getAccessToken() {
  try {
    return localStorage.getItem(storageKeys.accessToken)
  } catch {
    return null
  }
}

function saveAccessToken(accessToken) {
  try {
    localStorage.setItem(storageKeys.accessToken, accessToken)
  } catch {
    return false
  }

  return true
}

function removeAccessToken() {
  try {
    localStorage.removeItem(storageKeys.accessToken)
  } catch {
    // The in-memory auth state is still cleared by AuthProvider.
  }
}

export { getAccessToken, removeAccessToken, saveAccessToken }
