import { logout } from '../api/authApi'
import { storageKeys } from '../config/storageKeys'

export async function logoutAdmin() {
  try {
    await logout()
    localStorage.removeItem(storageKeys.accessToken)
    return { ok: true }
  } catch (error) {
    if (error.status === 401) {
      localStorage.removeItem(storageKeys.accessToken)
      return { ok: true }
    }

    return { ok: false, error }
  }
}
