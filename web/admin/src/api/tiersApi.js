import httpClient from './httpClient'

export async function getTierConfigs(signal) {
  const response = await httpClient.get('/tiers', { signal })
  return response.data.data
}
