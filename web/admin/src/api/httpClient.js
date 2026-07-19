import axios from 'axios'
import { normalizeApiError } from './apiError'

const accessTokenStorageKey = 'admin_access_token'
const languageStorageKey = 'language'

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/admin',
  timeout: 15_000,
  headers: {
    Accept: 'application/json',
  },
})

httpClient.interceptors.request.use((config) => {
  const accessToken = localStorage.getItem(accessTokenStorageKey)
  const language = localStorage.getItem(languageStorageKey) || 'en'

  config.headers['Accept-Language'] = language

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }

  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  (error) => Promise.reject(normalizeApiError(error)),
)

export default httpClient
