import { computed, ref } from 'vue'

interface AuthUser {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}

interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  user: AuthUser
}

const API_URL = 'http://localhost:5298'

const token = ref<string | null>(
  localStorage.getItem('auth_token')
)

const user = ref<AuthUser | null>(
  getStoredUser()
)

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

export function useAuth() {
  const isAuthenticated = computed(() => {
    return token.value !== null
  })

  async function login(
    email: string,
    password: string
  ): Promise<void> {
    const response = await fetch(
      `${API_URL}/api/auth/login`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          email,
          password
        })
      }
    )

    if (!response.ok) {
      let message = 'Identifiants incorrects.'

      try {
        const data = await response.json()

        if (data.message) {
          message = data.message
        }
      } catch {
        // Réponse non JSON.
      }

      throw new Error(message)
    }

    const data =
      await response.json() as AuthResponse

    if (!data.accessToken) {
      throw new Error(
        'Le serveur n\'a pas retourné de token.'
      )
    }

    token.value = data.accessToken
    user.value = data.user

    localStorage.setItem(
      'auth_token',
      data.accessToken
    )

    localStorage.setItem(
      'refresh_token',
      data.refreshToken
    )

    localStorage.setItem(
      'auth_user',
      JSON.stringify(data.user)
    )
  }

  async function register(
    email: string,
    password: string,
    firstName: string,
    lastName: string
  ): Promise<void> {
    const response = await fetch(
      `${API_URL}/api/auth/register`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          email,
          password,
          firstName,
          lastName
        })
      }
    )

    if (!response.ok) {
      let message = 'Impossible de créer le compte.'

      try {
        const data = await response.json()

        if (data.message) {
          message = data.message
        }
      } catch {
        // Réponse non JSON.
      }

      throw new Error(message)
    }

    const data =
      await response.json() as AuthResponse

    token.value = data.accessToken
    user.value = data.user

    localStorage.setItem(
      'auth_token',
      data.accessToken
    )

    localStorage.setItem(
      'refresh_token',
      data.refreshToken
    )

    localStorage.setItem(
      'auth_user',
      JSON.stringify(data.user)
    )
  }

  function logout() {
    token.value = null
    user.value = null

    localStorage.removeItem('auth_token')
    localStorage.removeItem('refresh_token')
    localStorage.removeItem('auth_user')
  }

  return {
    token,
    user,
    isAuthenticated,
    login,
    register,
    logout
  }
}
