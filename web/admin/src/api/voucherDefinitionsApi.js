import httpClient from './httpClient'

export async function getVoucherDefinitions({ page = 1, pageSize = 20, keyword, rewardType, validityType, publishType }, signal) {
  const params = { page, pageSize }
  if (keyword && keyword.trim()) {
    params.keyword = keyword.trim()
  }
  if (rewardType) {
    params.rewardType = rewardType
  }
  if (validityType) {
    params.validityType = validityType
  }
  if (publishType) {
    params.publishType = publishType
  }
  const response = await httpClient.get('/voucher-definitions', { params, signal })
  return response.data
}

export async function getVoucherDefinition(voucherDefinitionId, signal) {
  const response = await httpClient.get(`/voucher-definitions/${voucherDefinitionId}`, { signal })
  return response.data.data
}

export async function getVoucherDefinitionOptions(signal) {
  const response = await httpClient.get('/voucher-definitions/options', { signal })
  return response.data.data
}

export async function createVoucherDefinition(payload) {
  const response = await httpClient.post('/voucher-definitions', payload)
  return response.data.data
}

export async function uploadVoucherDefinitionBanner(file) {
  const formData = new FormData()
  formData.append('type', 'VOUCHER_DEFINITION_BANNER')
  formData.append('file', file)
  
  const response = await httpClient.post('/uploads/banners', formData)
  return response.data.data
}
