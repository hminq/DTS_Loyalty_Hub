import { logout } from '../api/authApi'

export async function logoutSession() {
  try {
    await logout()
    return { ok: true }
  } catch (error) {
    if (error.status === 401) {
      return { ok: true }
    }

    return { ok: false, error }
  }
}
