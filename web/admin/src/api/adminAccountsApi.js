import httpClient from './httpClient'

export async function getAdminAccounts({ page = 1, pageSize = 20, keyword, status, roleId } = {}) {
  const response = await httpClient.get('/admin-users', {
    params: {
      page,
      pageSize,
      keyword: keyword || undefined,
      status: status || undefined,
      roleId: roleId || undefined,
    },
  })

  return response.data
}

export async function getAdminAccount(adminId) {
  const response = await httpClient.get(`/admin-users/${adminId}`)
  return response.data.data
}

export async function createAdminAccount(payload) {
  const response = await httpClient.post('/admin-users', payload)
  return response.data.data
}
