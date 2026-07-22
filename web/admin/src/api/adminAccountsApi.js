import httpClient from './httpClient'

export async function getAdminAccounts(
  { page = 1, pageSize = 20, keyword, status, roleId } = {},
  signal,
) {
  const response = await httpClient.get('/admin-users', {
    params: {
      page,
      pageSize,
      keyword: keyword || undefined,
      status: status || undefined,
      roleId: roleId || undefined,
    },
    signal,
  })

  return response.data
}

export async function getAdminAccount(adminId, signal) {
  const response = await httpClient.get(`/admin-users/${adminId}`, { signal })
  return response.data.data
}

export async function createAdminAccount(payload) {
  const response = await httpClient.post('/admin-users', payload)
  return response.data.data
}

export async function updateAdminAccount(adminId, payload) {
  const response = await httpClient.put(`/admin-users/${adminId}`, payload)
  return response.data.data
}

export async function updateAdminAccountStatus(adminId, status) {
  await httpClient.patch(`/admin-users/${adminId}/status`, { status })
}

export async function revokeAdminAccountSession(adminId) {
  await httpClient.post(`/admin-users/${adminId}/revoke-session`)
}
