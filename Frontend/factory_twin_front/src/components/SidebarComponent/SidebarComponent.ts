import { useRoute, useRouter } from 'vue-router'

export interface SidebarNavItem {
  key: string
  label: string
  icon: string
  routeName?: string
}

type SidebarEmit = {
  (e: 'update:collapsed', value: boolean): void
  (e: 'logout'): void
}

const navItems: SidebarNavItem[] = [
  { key: 'home', label: 'Dashboard', icon: 'pi pi-home', routeName: 'home' },
  { key: 'lines', label: 'Lignes', icon: 'pi pi-sitemap', routeName: 'production-lines' },
  { key: 'products', label: 'Produits', icon: 'pi pi-box', routeName: 'products' },
  { key: 'machines', label: 'Machines', icon: 'pi pi-cog', routeName: 'machines' },
  { key: 'users', label: 'Utilisateurs', icon: 'pi pi-users', routeName: 'users' }
]

const settingsItem: SidebarNavItem = {
  key: 'settings',
  label: 'Paramètres',
  icon: 'pi pi-sliders-h',
  routeName: 'settings'
}

export function useSidebar(
  props: { collapsed: boolean },
  emit: SidebarEmit
) {
  const router = useRouter()
  const route = useRoute()

  function toggleCollapsed() {
    emit('update:collapsed', !props.collapsed)
  }

  function handleLogout() {
    emit('logout')
  }

  function navigate(item: SidebarNavItem) {
    if (item.routeName) {
      router.push({ name: item.routeName })
    }
  }

  function isActive(item: SidebarNavItem) {
    return !!item.routeName && route.name === item.routeName
  }

  return {
    navItems,
    toggleCollapsed,
    handleLogout,
    navigate,
    isActive,
    settingsItem
  }
}
