import {
  createRouter,
  createWebHistory
} from 'vue-router'

import LoginComponent from '../components/AuthComponent/LoginComponent.vue'

import { useAuth } from '../components/AuthComponent/useAuth'
import HomePageComponent from '@/components/HomePageComponent/HomePageComponent.vue'

const router = createRouter({
  history: createWebHistory(
    import.meta.env.BASE_URL
  ),

  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginComponent,

      meta: {
        guestOnly: true
      }
    },

    {
      path: '/',
      name: 'home',
      component: HomePageComponent,

      meta: {
        requiresAuth: true
      }
    }
  ]
})

router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()

  if (
    to.meta.requiresAuth &&
    !isAuthenticated.value
  ) {
    return {
      name: 'login'
    }
  }

  if (
    to.meta.guestOnly &&
    isAuthenticated.value
  ) {
    return {
      name: 'home'
    }
  }

  return true
})

export default router
