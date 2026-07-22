import httpClient from './httpClient'

export async function getRoles({ page = 1, pageSize = 20, keyword } = {}) {
  const response = await httpClient.get('/roles', {
    params: {
      page,
      pageSize,
      keyword: keyword || undefined,
    },
  })

  return response.data
}

export async function getRole(roleId) {
  const response = await httpClient.get(`/roles/${roleId}`)
  return response.data.data
}

export async function createRole(payload) {
  const response = await httpClient.post('/roles', payload)
  return response.data.data
}

export async function updateRole(roleId, payload) {
  const response = await httpClient.put(`/roles/${roleId}`, payload)
  return response.data.data
}

export async function deleteRole(roleId) {
  await httpClient.delete(`/roles/${roleId}`)
}

export async function searchRoleOptions(keyword = '', signal) {
  const response = await httpClient.get('/roles/options', {
    params: {
      keyword: keyword.trim() || undefined,
    },
    signal,
  })

  return response.data.data ?? []
}
