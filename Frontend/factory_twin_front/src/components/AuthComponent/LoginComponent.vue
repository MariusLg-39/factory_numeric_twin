<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

import { useAuth } from './useAuth'

const router = useRouter()
const { login } = useAuth()

const username = ref('')
const password = ref('')

const loading = ref(false)
const error = ref('')

async function handleLogin() {
  error.value = ''

  if (!username.value || !password.value) {
    error.value = 'Veuillez renseigner votre identifiant et votre mot de passe.'
    return
  }

  loading.value = true

  try {
    await login(
      username.value,
      password.value
    )

    await router.push({
      name: 'home'
    })
  } catch (err) {
    error.value =
      err instanceof Error
        ? err.message
        : 'Une erreur est survenue lors de la connexion.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="login-page">
    <PrimeCard class="login-card">
      <template #title>
        Connexion
      </template>

      <template #subtitle>
        Connectez-vous à votre espace
      </template>

      <template #content>
        <form
          class="login-form"
          @submit.prevent="handleLogin"
        >
          <PrimeMessage
            v-if="error"
            severity="error"
            :closable="false"
          >
            {{ error }}
          </PrimeMessage>

          <div class="form-field">
            <label for="username">
              Identifiant
            </label>

            <PrimeInputText
              id="username"
              v-model="username"
              name="username"
              type="text"
              autocomplete="username"
              placeholder="Votre identifiant"
              class="full-width"
              :disabled="loading"
            />
          </div>

          <div class="form-field">
            <label for="password">
              Mot de passe
            </label>

            <PrimePassword
              id="password"
              v-model="password"
              name="password"
              autocomplete="current-password"
              placeholder="Votre mot de passe"
              :feedback="false"
              :toggle-mask="true"
              :disabled="loading"
              input-class="full-width"
              class="full-width"
            />
          </div>

          <PrimeButton
            type="submit"
            label="Se connecter"
            icon="pi pi-sign-in"
            :loading="loading"
            :disabled="loading"
            class="full-width"
          />
        </form>
      </template>
    </PrimeCard>
  </main>
</template>

<style scoped src="./LoginComponent.css"/>
