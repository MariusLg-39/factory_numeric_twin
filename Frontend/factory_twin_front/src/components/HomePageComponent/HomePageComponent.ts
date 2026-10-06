import { useAuth } from '../AuthComponent/useAuth'

export function useHomePage() {
  const { user } = useAuth()

  return { user }
}
