import axios from 'axios'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/'

export const backendOrigin =
  import.meta.env.VITE_BACKEND_ORIGIN || 'http://localhost:5272'

export const apiClient = axios.create({
  baseURL: apiBaseUrl === '' ? '/' : apiBaseUrl,
  timeout: 12000
})
