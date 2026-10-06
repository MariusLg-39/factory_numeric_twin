import {
  createRouter,
  createWebHistory
} from 'vue-router'

import LoginComponent from '../components/AuthComponent/LoginComponent.vue'

import { useAuth } from '../components/AuthComponent/useAuth'
import HomePageComponent from '@/components/HomePageComponent/HomePageComponent.vue'
import ProductionLineComponent from '@/components/ProductionLineComponent/ProductionLineComponent.vue'
import MainLayoutComponent from '@/components/MainLayoutComponent/MainLayoutComponent.vue'
import ProductComponent from '@/components/ProductComponent/ProductComponent.vue'
import MachineComponent from '@/components/MachineComponent/MachineComponent.vue'
import UserComponent from '@/components/UserComponent/UserComponent.vue'
import SettingsComponent from '@/components/SettingsComponent/SettingsComponent.vue'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),

  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginComponent,
      meta: { guestOnly: true }
    },

    {
      path: '/',
      component: MainLayoutComponent,
      meta: { requiresAuth: true }, // hérité par tous les enfants
      children: [
        {
          path: '',
          name: 'home',
          component: HomePageComponent,
          meta: {
            title: 'Dashboard',
            subtitle: "Vue d'ensemble de votre usine"
          }
        },
        {
          path: 'production-lines',
          name: 'production-lines',
          component: ProductionLineComponent,
          meta: {
            title: 'Lignes de production',
            subtitle: 'Gérez vos lignes'
          }
        },
        {
          path: 'products',
          name: 'products',
          component: ProductComponent,
          meta: {
            title: 'Produits',
            subtitle: 'Gérez vos produits'
          }
        },
        {
          path: 'machines',
          name: 'machines',
          component: MachineComponent,
          meta: {
            title: 'Machines',
            subtitle: 'Gérez vos machines'
          }
        },
        {
          path: 'users',
          name: 'users',
          component: UserComponent,
          meta: {
            title: 'Utilisateurs',
            subtitle: 'Gérez vos utilisateurs'
          },
        },
        {
          path: 'settings',
          name: 'settings',
          component: SettingsComponent,
          meta: {
            title: 'Paramètres',
            subtitle: 'Gérez vos paramètres'
          },
        }
      ]
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
