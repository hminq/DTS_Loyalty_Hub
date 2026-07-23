import httpClient from './httpClient'

export async function getTierConfigs(signal) {
  const response = await httpClient.get('/tiers', { signal })
  return response.data.data
}

export async function getTierConfig(tierConfigId, signal) {
  const response = await httpClient.get(`/tiers/${tierConfigId}`, { signal })
  return response.data.data
}

export async function createTierConfig(payload) {
  const response = await httpClient.post('/tiers', payload)
  return response.data.data
}

export async function updateTierConfig(tierConfigId, payload) {
  const response = await httpClient.put(`/tiers/${tierConfigId}`, payload)
  return response.data.data
}
