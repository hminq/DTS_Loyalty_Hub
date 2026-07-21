import httpClient from './httpClient'

let permissionsRequest

export function getPermissions({ force = false } = {}) {
  if (!permissionsRequest || force) {
    permissionsRequest = httpClient
      .get('/permissions')
      .then((response) => response.data.data)
      .catch((error) => {
        permissionsRequest = undefined
        throw error
      })
  }

  return permissionsRequest
}
