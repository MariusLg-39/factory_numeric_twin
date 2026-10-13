import axios, {
  type InternalAxiosRequestConfig,
} from 'axios'

interface AuthResponse {
  accessToken: string
  accessTokenExpiresAt: string
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
    roles: string[]
  }
}

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

let refreshPromise: Promise<string> | null = null

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as
      | RetryableRequestConfig
      | undefined

    if (
      error.response?.status !== 401 ||
      !originalRequest ||
      originalRequest._retry ||
      originalRequest.url?.includes('/auth/login') ||
      originalRequest.url?.includes('/auth/refresh') ||
      originalRequest.url?.includes('/auth/register')
    ) {
      return Promise.reject(error)
    }

    if (!localStorage.getItem('auth_token')) {
      clearAuthStorage()
      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      if (!refreshPromise) {
        refreshPromise = api
          .post<AuthResponse>('/auth/refresh')
          .then((response) => {
            const data = response.data

            if (!data.accessToken) {
              throw new Error(
                'Réponse de renouvellement invalide.',
              )
            }

            localStorage.setItem(
              'auth_token',
              data.accessToken,
            )

            if (data.user) {
              localStorage.setItem(
                'auth_user',
                JSON.stringify(data.user),
              )
            }

            window.dispatchEvent(
              new CustomEvent('auth:updated', {
                detail: data,
              }),
            )

            return data.accessToken
          })
          .catch((refreshError) => {
            clearAuthStorage()
            throw refreshError
          })
          .finally(() => {
            refreshPromise = null
          })
      }

      const newAccessToken = await refreshPromise

      originalRequest.headers.Authorization =
        `Bearer ${newAccessToken}`

      return api(originalRequest)
    } catch (refreshError) {
      return Promise.reject(refreshError)
    }
  },
)

function clearAuthStorage() {
  localStorage.removeItem('auth_token')
  localStorage.removeItem('auth_user')

  window.dispatchEvent(new Event('auth:cleared'))
}

export default api
