import httpClient from './httpClient'

export async function getAuditLogs(params, signal) {
  const response = await httpClient.get('/audit-logs', {
    params: {
      page: params.page,
      pageSize: params.pageSize,
      fromDate: params.fromDate || undefined,
      toDate: params.toDate || undefined,
      entityType: params.entityType || undefined,
      action: params.action || undefined,
    },
    signal,
  })

  return response.data
}

export async function getAuditLogFilterOptions(signal) {
  const response = await httpClient.get('/audit-logs/options', { signal })
  return response.data.data
}
