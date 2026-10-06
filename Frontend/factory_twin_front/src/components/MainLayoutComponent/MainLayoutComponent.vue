<script setup lang="ts">
import { useMainLayout } from './MainLayoutComponent'
import SidebarComponent from '../SidebarComponent/SidebarComponent.vue'

const {
  user,
  sidebarCollapsed,
  handleLogout
} = useMainLayout()
</script>

<template>
  <main class="home-page">
    <SidebarComponent
      v-model:collapsed="sidebarCollapsed"
      @logout="handleLogout"
    />

    <section
      class="main-content"
      :class="{ 'sidebar-collapsed': sidebarCollapsed }"
    >
      <header class="topbar">
        <div>
          <h1>{{ $route.meta.title }}</h1>
          <p>{{ $route.meta.subtitle }}</p>
        </div>

        <div v-if="user" class="user-info">
          <div class="user-avatar">
            <i class="pi pi-user" />
          </div>

          <div class="user-details">
            <strong>{{ user.firstName }} {{ user.lastName }}</strong>
            <span>{{ user.email }}</span>
          </div>
        </div>
      </header>

      <router-view />
    </section>
  </main>
</template>

<style scoped src="./MainLayoutComponent.css" />
