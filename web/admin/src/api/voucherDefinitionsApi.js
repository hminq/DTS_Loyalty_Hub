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

export async function getVoucherImportTemplate() {
  const response = await httpClient.get('/voucher-definitions/import-template')
  return response.data.data
}

export async function createVoucherDefinition(payload) {
  const response = await httpClient.post('/voucher-definitions', payload)
  return response.data.data
}

export async function createVoucherPoolImportUploadUrl(voucherDefinitionId, file) {
  const response = await httpClient.post(
    `/voucher-definitions/${voucherDefinitionId}/pool-imports/upload-url`,
    {
      fileName: file.name,
      fileSizeBytes: file.size,
    },
  )
  return response.data.data
}

export async function uploadVoucherPoolCsvToS3(upload, file, signal) {
  const response = await fetch(upload.uploadUrl, {
    method: upload.method,
    headers: {
      'Content-Type': upload.contentType,
    },
    body: file,
    signal,
  })

  if (!response.ok) {
    throw new Error(`S3_UPLOAD_FAILED_${response.status}`)
  }
}

export async function createVoucherPoolImportJob(voucherDefinitionId, importFileKey) {
  const response = await httpClient.post(
    `/voucher-definitions/${voucherDefinitionId}/pool-imports`,
    { importFileKey },
  )
  return response.data.data
}

export async function uploadVoucherDefinitionBanner(file) {
  const formData = new FormData()
  formData.append('type', 'VOUCHER_DEFINITION_BANNER')
  formData.append('file', file)
  
  const response = await httpClient.post('/uploads/banners', formData)
  return response.data.data
}
