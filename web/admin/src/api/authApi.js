import httpClient from './httpClient'

export async function login(credentials) {
  const response = await httpClient.post('/login', credentials)
  return response.data.data
}

export async function logout() {
  await httpClient.post('/logout')
}

export async function getCurrentAdmin() {
  const response = await httpClient.get('/me')
  return response.data.data
}
