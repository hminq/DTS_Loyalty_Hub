import httpClient from './httpClient'

export async function getCampaigns({ page = 1, pageSize = 20, keyword, status, eventType } = {}, signal) {
  const params = { page, pageSize }
  if (keyword && keyword.trim()) {
    params.keyword = keyword.trim()
  }
  if (status) {
    params.status = status
  }
  if (eventType) {
    params.eventType = eventType
  }
  const response = await httpClient.get('/campaigns', { params, signal })
  return response.data
}

export async function getCampaignOptions(signal) {
  const response = await httpClient.get('/campaigns/options', { signal })
  return response.data.data
}
