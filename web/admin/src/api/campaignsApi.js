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

export async function uploadCampaignBanner(file, signal) {
  const formData = new FormData()
  formData.append('type', 'CAMPAIGN_BANNER')
  formData.append('file', file)

  const response = await httpClient.post('/uploads/banners', formData, { signal })
  return response.data.data
}

export async function createCampaign(payload, signal) {
  const response = await httpClient.post('/campaigns', payload, { signal })
  return response.data.data
}

export async function getCampaign(campaignId, signal) {
  if (!campaignId) {
    throw new Error('campaignId is required')
  }
  const response = await httpClient.get(`/campaigns/${encodeURIComponent(campaignId)}`, { signal })
  return response.data.data
}

export async function updateCampaign(campaignId, payload, signal) {
  if (!campaignId) {
    throw new Error('campaignId is required')
  }
  const response = await httpClient.put(`/campaigns/${encodeURIComponent(campaignId)}`, payload, { signal })
  return response.data.data
}

export async function deleteCampaign(campaignId, signal) {
  if (!campaignId) {
    throw new Error('campaignId is required')
  }
  const response = await httpClient.delete(`/campaigns/${encodeURIComponent(campaignId)}`, { signal })
  return response.data.data
}

export async function getCampaignAction(campaignId, actionId, signal) {
  if (!campaignId || !actionId) {
    throw new Error('campaignId and actionId are required')
  }
  const response = await httpClient.get(`/campaigns/${encodeURIComponent(campaignId)}/actions/${encodeURIComponent(actionId)}`, { signal })
  return response.data.data
}

export async function createCampaignAction(campaignId, payload, signal) {
  if (!campaignId) {
    throw new Error('campaignId is required')
  }
  const response = await httpClient.post(`/campaigns/${encodeURIComponent(campaignId)}/actions`, payload, { signal })
  return response.data.data
}

export async function updateCampaignAction(campaignId, actionId, payload, signal) {
  if (!campaignId || !actionId) {
    throw new Error('campaignId and actionId are required')
  }
  const response = await httpClient.put(`/campaigns/${encodeURIComponent(campaignId)}/actions/${encodeURIComponent(actionId)}`, payload, { signal })
  return response.data.data
}

export async function deleteCampaignAction(campaignId, actionId, signal) {
  if (!campaignId || !actionId) {
    throw new Error('campaignId and actionId are required')
  }
  const response = await httpClient.delete(`/campaigns/${encodeURIComponent(campaignId)}/actions/${encodeURIComponent(actionId)}`, { signal })
  return response.data.data
}

export async function activateCampaign(campaignId, signal) {
  if (!campaignId) {
    throw new Error('campaignId is required')
  }
  const response = await httpClient.post(`/campaigns/${encodeURIComponent(campaignId)}/activate`, {}, { signal })
  return response.data.data
}

export async function cancelCampaign(campaignId, signal) {
  if (!campaignId) {
    throw new Error('campaignId is required')
  }
  const response = await httpClient.post(`/campaigns/${encodeURIComponent(campaignId)}/cancel`, {}, { signal })
  return response.data.data
}
