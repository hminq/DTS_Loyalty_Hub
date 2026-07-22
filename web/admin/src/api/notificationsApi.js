import httpClient from './httpClient'

export async function getNotificationTemplates({ page = 1, pageSize = 20, keyword, eventTypeCode, channel, language, isActive } = {}) {
  const response = await httpClient.get('/notification-templates', {
    params: {
      page,
      pageSize,
      keyword: keyword || undefined,
      eventTypeCode: eventTypeCode || undefined,
      channel: channel || undefined,
      language: language || undefined,
      isActive: isActive !== undefined ? isActive : undefined,
    },
  })

  return response.data
}

export async function getNotificationTemplate(templateId) {
  const response = await httpClient.get(`/notification-templates/${templateId}`)
  return response.data.data
}

export async function createNotificationTemplate(payload) {
  const response = await httpClient.post('/notification-templates', payload)
  return response.data.data
}

export async function updateNotificationTemplate(templateId, payload) {
  const response = await httpClient.put(`/notification-templates/${templateId}`, payload)
  return response.data.data
}

export async function toggleTemplateStatus(templateId) {
  const response = await httpClient.patch(`/notification-templates/${templateId}/toggle-status`)
  return response.data.data
}

export async function getNotificationEventTypes({ searchKeyword } = {}) {
  const response = await httpClient.get('/notification-event-types', {
    params: {
      searchKeyword: searchKeyword || undefined,
    },
  })
  return response.data.data
}
