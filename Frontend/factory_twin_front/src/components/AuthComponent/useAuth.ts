import { computed, ref } from 'vue'
import axios from 'axios'
import api from '../../services/api'

interface AuthUser {
  id: string
  email: string
  firstName: string
  lastName: string
  roles?: string[]
}

interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  user: AuthUser
}

function getStoredUser(): AuthUser | null {
  const storedUser = localStorage.getItem('auth_user')

  if (!storedUser) {
    return null
  }

  try {
    return JSON.parse(storedUser) as AuthUser
  } catch {
    return null
  }
}

const token = ref<string | null>(
  localStorage.getItem('auth_token'),
)

const user = ref<AuthUser | null>(getStoredUser())

window.addEventListener('auth:cleared', () => {
  token.value = null
  user.value = null
})

window.addEventListener('auth:updated', ((event: CustomEvent<AuthResponse>) => {
  token.value = event.detail.accessToken
  user.value = event.detail.user
}) as EventListener)

function saveAuth(data: AuthResponse) {
  token.value = data.accessToken
  user.value = data.user

  localStorage.setItem('auth_token', data.accessToken)
  localStorage.setItem('refresh_token', data.refreshToken)
  localStorage.setItem('auth_user', JSON.stringify(data.user))
}

function clearAuth() {
  token.value = null
  user.value = null

  localStorage.removeItem('auth_token')
  localStorage.removeItem('refresh_token')
  localStorage.removeItem('auth_user')
}

function getErrorMessage(
  error: unknown,
  defaultMessage: string,
): string {
  if (axios.isAxiosError(error)) {
    return error.response?.data?.message ?? defaultMessage
  }

  return error instanceof Error
    ? error.message
    : defaultMessage
}

export function useAuth() {
  const isAuthenticated = computed(() => token.value !== null)

  async function login(
    email: string,
    password: string,
  ): Promise<void> {
    try {
      const response = await api.post<AuthResponse>(
        '/auth/login',
        { email, password },
      )

      if (!response.data.accessToken || !response.data.refreshToken) {
        throw new Error('Le serveur n’a pas retourné les jetons attendus.')
      }

      saveAuth(response.data)
    } catch (error) {
      throw new Error(
        getErrorMessage(error, 'Identifiants incorrects.'),
      )
    }
  }

  async function register(
    email: string,
    password: string,
    firstName: string,
    lastName: string,
  ): Promise<void> {
    try {
      await api.post('/auth/register', {
        email,
        password,
        firstName,
        lastName,
      })
    } catch (error) {
      throw new Error(
        getErrorMessage(error, 'Impossible de créer le compte.'),
      )
    }
  }

  async function logout(): Promise<void> {
    const refreshToken = localStorage.getItem('refresh_token')

    try {
      if (refreshToken && token.value) {
        await api.post('/auth/revoke', { refreshToken })
      }
    } catch {
      // La session locale doit être effacée même si la révocation échoue.
    } finally {
      clearAuth()
    }
  }

  return {
    token,
    user,
    isAuthenticated,
    login,
    register,
    logout,
  }
}
