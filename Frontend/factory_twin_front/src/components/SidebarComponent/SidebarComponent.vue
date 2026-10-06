<script setup lang="ts">
import { useSidebar } from './SidebarComponent'

const props = defineProps<{
  collapsed: boolean
}>()

const emit = defineEmits<{
  (e: 'update:collapsed', value: boolean): void
  (e: 'logout'): void
}>()

const {
  navItems,
  toggleCollapsed,
  handleLogout,
  navigate,
  isActive,
  settingsItem
} = useSidebar(props, emit)
</script>

<template>
  <aside
    class="sidebar"
    :class="{ collapsed }"
  >
    <!-- Logo -->
    <div class="sidebar-header">
      <div class="logo">
        <i class="pi pi-building" />
      </div>

      <span
        v-if="!collapsed"
        class="logo-text"
      >
        Factory Twin
      </span>

      <PrimeButton
        class="collapse-button"
        text
        rounded
        :icon="collapsed ? 'pi pi-angle-right' : 'pi pi-angle-left'"
        @click="toggleCollapsed"
      />
    </div>

    <!-- Navigation -->
    <nav class="sidebar-nav">
      <PrimeButton
        v-for="item in navItems"
        :key="item.key"
        text
        class="nav-item"
        :class="{ collapsed, active: isActive(item) }"
        v-tooltip.right="collapsed ? item.label : ''"
        @click="navigate(item)"
      >
        <i :class="item.icon" />
        <span v-if="!collapsed">{{ item.label }}</span>
      </PrimeButton>
    </nav>

    <div class="sidebar-settings">
        <PrimeButton
          text
          class="nav-item"
          :class="{ collapsed, active: isActive(settingsItem) }"
          v-tooltip.right="collapsed ? settingsItem.label : ''"
          @click="navigate(settingsItem)"
        >
          <i :class="settingsItem.icon" />
          <span v-if="!collapsed">{{ settingsItem.label }}</span>
        </PrimeButton>
      </div>

    <!-- Logout -->
    <div class="sidebar-footer">
      <PrimeButton
        text
        severity="secondary"
        class="nav-item logout-item"
        :class="{ collapsed }"
        v-tooltip.right="collapsed ? 'Déconnexion' : ''"
        @click="handleLogout"
      >
        <i class="pi pi-sign-out" />
        <span v-if="!collapsed">Déconnexion</span>
      </PrimeButton>
    </div>
  </aside>
</template>

<style scoped src="./SidebarComponent.css" />
