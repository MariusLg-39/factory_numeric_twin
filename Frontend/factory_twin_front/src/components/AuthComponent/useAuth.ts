import { computed, ref } from 'vue'

const token = ref<string | null>(
  localStorage.getItem('auth_token')
)

export function useAuth() {
  const isAuthenticated = computed(() => {
    return token.value !== null
  })

  async function login(
    username: string,
    password: string
  ): Promise<void> {
    // Simulation de connexion
    // À remplacer plus tard par appel API
    if (username === 'admin' && password === 'admin') {
      const fakeToken = 'fake-jwt-token'

      token.value = fakeToken
      localStorage.setItem('auth_token', fakeToken)

      return
    }

    throw new Error('Identifiants incorrects')
  }

  function logout() {
    token.value = null
    localStorage.removeItem('auth_token')
  }

  return {
    token,
    isAuthenticated,
    login,
    logout
  }
}
