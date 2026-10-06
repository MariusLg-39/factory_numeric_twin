import { ref } from 'vue'
import { useRouter } from 'vue-router'

import { useAuth } from '../AuthComponent/useAuth'

export function useMainLayout() {
  const router = useRouter()
  const { user, logout } = useAuth()

  const sidebarCollapsed = ref(false)

  async function handleLogout() {
    logout()
    await router.push({ name: 'login' })
  }

  return {
    user,
    sidebarCollapsed,
    handleLogout
  }
}
