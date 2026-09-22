<script setup lang="ts">
import { useHomePage } from './HomePageComponent'

const {
  user,
  handleLogout
} = useHomePage()
</script>

<template>
  <main class="home-page">
    <PrimeCard class="home-card">
      <template #title>
        Bienvenue !
      </template>

      <template #subtitle>
        Factory Twin
      </template>

      <template #content>
        <div class="home-content">
          <p v-if="user">
            Bonjour
            <strong>
              {{ user.firstName }}
              {{ user.lastName }}
            </strong>
            !
          </p>

          <p v-if="user">
            Vous êtes connecté avec
            <strong>{{ user.email }}</strong>.
          </p>

          <p v-if="user?.roles?.length">
            Rôle :
            <strong>
              {{ user.roles.join(', ') }}
            </strong>
          </p>

          <PrimeButton
            label="Se déconnecter"
            icon="pi pi-sign-out"
            severity="secondary"
            @click="handleLogout"
          />
        </div>
      </template>
    </PrimeCard>
  </main>
</template>

<style scoped src="./HomePageComponent.css" />
