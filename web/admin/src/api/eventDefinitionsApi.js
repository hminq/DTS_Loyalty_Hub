import httpClient from './httpClient'

export async function getEventDefinitions({ page = 1, pageSize = 20, keyword, status } = {}, signal) {
  const params = { page, pageSize }
  if (keyword && keyword.trim()) {
    params.keyword = keyword.trim()
  }
  if (status) {
    params.status = status
  }
  const response = await httpClient.get('/event-definitions', { params, signal })
  return response.data
}

export async function getEventDefinitionOptions(signal) {
  const response = await httpClient.get('/event-definitions/options', { signal })
  return response.data.data
}

export async function getEventDefinition(eventTypeId, signal) {
  if (!eventTypeId) {
    throw new Error('eventTypeId is required')
  }
  const response = await httpClient.get(`/event-definitions/${encodeURIComponent(eventTypeId)}`, { signal })
  return response.data.data
}

export async function getEventDefinitionVersion(eventTypeId, eventTypeVersionId, signal) {
  if (!eventTypeId || !eventTypeVersionId) {
    throw new Error('eventTypeId and eventTypeVersionId are required')
  }
  const response = await httpClient.get(
    `/event-definitions/${encodeURIComponent(eventTypeId)}/versions/${encodeURIComponent(eventTypeVersionId)}`,
    { signal },
  )
  return response.data.data
}

export async function createEventDefinition(payload, signal) {
  const response = await httpClient.post('/event-definitions', payload, { signal })
  return response.data.data
}

export async function updateEventDefinition(eventTypeId, payload, signal) {
  if (!eventTypeId) {
    throw new Error('eventTypeId is required')
  }
  const response = await httpClient.put(`/event-definitions/${encodeURIComponent(eventTypeId)}`, payload, { signal })
  return response.data.data
}

export async function updateEventDefinitionVersion(eventTypeId, eventTypeVersionId, payload, signal) {
  if (!eventTypeId || !eventTypeVersionId) {
    throw new Error('eventTypeId and eventTypeVersionId are required')
  }
  const response = await httpClient.put(
    `/event-definitions/${encodeURIComponent(eventTypeId)}/versions/${encodeURIComponent(eventTypeVersionId)}`,
    payload,
    { signal },
  )
  return response.data.data
}

export async function publishEventDefinitionVersion(eventTypeId, eventTypeVersionId, signal) {
  if (!eventTypeId || !eventTypeVersionId) {
    throw new Error('eventTypeId and eventTypeVersionId are required')
  }
  const response = await httpClient.post(
    `/event-definitions/${encodeURIComponent(eventTypeId)}/versions/${encodeURIComponent(eventTypeVersionId)}/publish`,
    {},
    { signal },
  )
  return response.data.data
}

export async function cloneEventDefinitionVersion(eventTypeId, sourceVersionId, signal) {
  if (!eventTypeId || !sourceVersionId) {
    throw new Error('eventTypeId and sourceVersionId are required')
  }
  const response = await httpClient.post(
    `/event-definitions/${encodeURIComponent(eventTypeId)}/versions/${encodeURIComponent(sourceVersionId)}/clone`,
    {},
    { signal },
  )
  return response.data.data
}

export async function retireEventDefinitionVersion(eventTypeId, eventTypeVersionId, signal) {
  if (!eventTypeId || !eventTypeVersionId) {
    throw new Error('eventTypeId and eventTypeVersionId are required')
  }
  const response = await httpClient.post(
    `/event-definitions/${encodeURIComponent(eventTypeId)}/versions/${encodeURIComponent(eventTypeVersionId)}/retire`,
    {},
    { signal },
  )
  return response.data.data
}

export async function retireEventDefinition(eventTypeId, signal) {
  if (!eventTypeId) {
    throw new Error('eventTypeId is required')
  }
  const response = await httpClient.post(
    `/event-definitions/${encodeURIComponent(eventTypeId)}/retire`,
    {},
    { signal },
  )
  return response.data.data
}
