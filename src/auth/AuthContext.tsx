import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import axios from 'axios'
import { API_BASE_URL, API_ENDPOINTS } from '../config'
import { apiClient, getAccessToken, getRefreshToken, storeTokens, clearTokens, setOnAuthFailure } from './apiClient'

interface Employee {
  employeeId: string
  displayName: string
}

interface AuthContextValue {
  employee: Employee | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string
  login: (username: string, password: string) => Promise<boolean>
  loginWithSso: () => Promise<boolean>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [employee, setEmployee] = useState<Employee | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  const applySession = (data: { accessToken: string; refreshToken: string; employeeId: string; displayName: string }) => {
    storeTokens(data.accessToken, data.refreshToken)
    setEmployee({ employeeId: data.employeeId, displayName: data.displayName })
  }

  const logout = () => {
    const refreshToken = getRefreshToken()
    clearTokens()
    setEmployee(null)
    if (refreshToken) {
      axios.post(`${API_BASE_URL}${API_ENDPOINTS.LOGOUT}`, { refreshToken }).catch(() => {})
    }
  }

  useEffect(() => {
    setOnAuthFailure(() => setEmployee(null))

    if (!getAccessToken()) {
      setIsLoading(false)
      return
    }

    apiClient
      .get(API_ENDPOINTS.ME)
      .then((res) => setEmployee({ employeeId: res.data.employeeId, displayName: res.data.displayName }))
      .catch(() => clearTokens())
      .finally(() => setIsLoading(false))
  }, [])

  const login = async (username: string, password: string) => {
    setError('')
    try {
      const res = await axios.post(`${API_BASE_URL}${API_ENDPOINTS.LOGIN}`, { username, password })
      applySession(res.data)
      return true
    } catch (err: any) {
      setError(err.response?.data?.message ?? 'Login failed')
      return false
    }
  }

  const loginWithSso = async () => {
    setError('')
    try {
      const res = await axios.post(`${API_BASE_URL}${API_ENDPOINTS.SSO}`)
      applySession(res.data)
      return true
    } catch (err: any) {
      setError(err.response?.data?.message ?? 'SSO sign-in failed')
      return false
    }
  }

  return (
    <AuthContext.Provider value={{ employee, isAuthenticated: !!employee, isLoading, error, login, loginWithSso, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
