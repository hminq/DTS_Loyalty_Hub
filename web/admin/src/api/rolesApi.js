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

export async function getRoleOptions({ page = 1, pageSize = 20, keyword } = {}) {
  const response = await httpClient.get('/roles/options', {
    params: {
      page,
      pageSize,
      keyword: keyword || undefined,
    },
  })

  return response.data
}

export async function getAllRoleOptions() {
  const roles = []
  let page = 1
  let totalPages = 1

  do {
    const response = await getRoleOptions({ page, pageSize: 100 })
    roles.push(...(response.data ?? []))
    totalPages = Math.max(response.meta?.totalPages ?? 1, 1)
    page += 1
  } while (page <= totalPages)

  return [...new Map(roles.map((role) => [role.roleId, role])).values()]
}
