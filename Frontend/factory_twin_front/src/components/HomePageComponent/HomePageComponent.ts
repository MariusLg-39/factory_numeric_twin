
import { useRouter } from 'vue-router'

import { useAuth } from '../AuthComponent/useAuth'

export function useHomePage() {
  const router = useRouter()

  const {
    user,
    isAuthenticated,
    logout
  } = useAuth()

  async function handleLogout() {
    logout()

    await router.push({
      name: 'login'
    })
  }

  return {
    user,
    isAuthenticated,
    handleLogout
  }
}
