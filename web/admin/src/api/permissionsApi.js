import httpClient from './httpClient'

export async function getPermissions() {
  const response = await httpClient.get('/permissions')
  return response.data.data
}
