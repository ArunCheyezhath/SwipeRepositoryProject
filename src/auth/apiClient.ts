import axios from 'axios'
import { API_BASE_URL, API_ENDPOINTS } from '../config'

const ACCESS_TOKEN_KEY = 'timesmart_access_token'
const REFRESH_TOKEN_KEY = 'timesmart_refresh_token'

export function getAccessToken() {
  return localStorage.getItem(ACCESS_TOKEN_KEY)
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

export function storeTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
}

export function clearTokens() {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

export const apiClient = axios.create({ baseURL: API_BASE_URL })

let onAuthFailure: (() => void) | null = null
export function setOnAuthFailure(handler: () => void) {
  onAuthFailure = handler
}

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Serializes concurrent 401s onto a single in-flight refresh call instead of
// firing one refresh request per failed request.
let refreshPromise: Promise<string | null> | null = null

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) return null

  if (!refreshPromise) {
    refreshPromise = axios
      .post(`${API_BASE_URL}${API_ENDPOINTS.REFRESH}`, { refreshToken })
      .then((res) => {
        storeTokens(res.data.accessToken, res.data.refreshToken)
        return res.data.accessToken as string
      })
      .catch(() => {
        clearTokens()
        return null
      })
      .finally(() => {
        refreshPromise = null
      })
  }
  return refreshPromise
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry && getRefreshToken()) {
      original._retry = true
      const newToken = await refreshAccessToken()
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`
        return apiClient(original)
      }
    }
    if (error.response?.status === 401) {
      clearTokens()
      onAuthFailure?.()
    }
    return Promise.reject(error)
  }
)
