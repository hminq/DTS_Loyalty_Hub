import axios from 'axios'
import { storageKeys } from '../config/storageKeys'
import { normalizeApiError } from './apiError'

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15_000,
  headers: {
    Accept: 'application/json',
  },
})

httpClient.interceptors.request.use((config) => {
  const accessToken = localStorage.getItem(storageKeys.accessToken)
  const language = localStorage.getItem(storageKeys.language) || 'en' //default language

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
