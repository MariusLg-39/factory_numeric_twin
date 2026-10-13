<template>
  <div>
    <h1>Gestion des utilisateurs</h1>

    <p v-if="loading">Chargement des utilisateurs...</p>
    <p v-if="errorMessage" style="color: red">
      {{ errorMessage }}
    </p>
    <p v-if="successMessage" style="color: green">
      {{ successMessage }}
    </p>

    <!-- Formulaire d'ajout -->
    <section>
      <h2>Ajouter un utilisateur</h2>

      <form @submit.prevent="addUser">
        <div>
          <label for="firstName">Prénom :</label>
          <input
            id="firstName"
            v-model="newUser.firstName"
            type="text"
            required
            maxlength="100"
          />
        </div>

        <div>
          <label for="lastName">Nom :</label>
          <input
            id="lastName"
            v-model="newUser.lastName"
            type="text"
            required
            maxlength="100"
          />
        </div>

        <div>
          <label for="email">Email :</label>
          <input
            id="email"
            v-model="newUser.email"
            type="email"
            required
          />
        </div>

        <div>
          <label for="userName">Nom d'utilisateur :</label>
          <input
            id="userName"
            v-model="newUser.userName"
            type="text"
            required
            minlength="3"
            maxlength="100"
          />
        </div>

        <div>
          <label for="password">Mot de passe :</label>
          <input
            id="password"
            v-model="newUser.password"
            type="password"
            required
            minlength="12"
          />
        </div>

        <div>
          <label>
            <input
              v-model="newUser.isAdmin"
              type="checkbox"
            />
            Administrateur
          </label>
        </div>

        <button type="submit" :disabled="submitting">
          {{ submitting ? 'Ajout en cours...' : 'Ajouter' }}
        </button>
      </form>
    </section>

    <hr />

    <!-- Utilisateurs classiques -->
    <section>
      <h2>Utilisateurs classiques ({{ regularUsers.length }})</h2>

      <p v-if="regularUsers.length === 0">
        Aucun utilisateur classique.
      </p>

      <table v-else border="1" cellpadding="8">
        <thead>
          <tr>
            <th>Prénom</th>
            <th>Nom</th>
            <th>Email</th>
            <th>Nom d'utilisateur</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="user in regularUsers" :key="user.id">
            <td>{{ user.firstName }}</td>
            <td>{{ user.lastName }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.userName }}</td>
            <td>
              <button
                type="button"
                @click="deleteUser(user.id)"
                :disabled="deletingId === user.id"
              >
                {{ deletingId === user.id ? 'Suppression...' : 'Supprimer' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <hr />

    <!-- Administrateurs -->
    <section>
      <h2>Administrateurs ({{ adminUsers.length }})</h2>

      <p v-if="adminUsers.length === 0">
        Aucun administrateur.
      </p>

      <table v-else border="1" cellpadding="8">
        <thead>
          <tr>
            <th>Prénom</th>
            <th>Nom</th>
            <th>Email</th>
            <th>Nom d'utilisateur</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="user in adminUsers" :key="user.id">
            <td>{{ user.firstName }}</td>
            <td>{{ user.lastName }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.userName }}</td>
            <td>
              <button
                type="button"
                @click="deleteUser(user.id)"
                :disabled="deletingId === user.id"
              >
                {{ deletingId === user.id ? 'Suppression...' : 'Supprimer' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <button type="button" @click="loadUsers" :disabled="loading">
      Actualiser la liste
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import axios from 'axios'
import api from '@/services/api'

interface User {
  id: string
  email: string
  userName: string
  firstName: string
  lastName: string
  isActive: boolean
  roles: string[]
}

interface CreateUserRequest {
  firstName: string
  lastName: string
  email: string
  userName: string
  password: string
  isAdmin: boolean
}

const users = ref<User[]>([])
const loading = ref(false)
const submitting = ref(false)
const deletingId = ref<string | null>(null)
const errorMessage = ref('')
const successMessage = ref('')

const newUser = reactive<CreateUserRequest>({
  firstName: '',
  lastName: '',
  email: '',
  userName: '',
  password: '',
  isAdmin: false,
})

const regularUsers = computed(() =>
  users.value.filter(user => !user.roles?.includes('Admin'))
)

const adminUsers = computed(() =>
  users.value.filter(user => user.roles?.includes('Admin'))
)

function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data

    if (typeof data === 'string' && data.length > 0) {
      return data
    }

    if (typeof data?.message === 'string') {
      return data.message
    }

    if (typeof data?.title === 'string') {
      return data.title
    }

    if (data?.errors && typeof data.errors === 'object') {
      return Object.values(data.errors as Record<string, string[]>)
        .flat()
        .join(' ')
    }

    if (error.response?.status === 401) {
      return 'Tu dois être connecté pour effectuer cette action.'
    }

    if (error.response?.status === 403) {
      return 'Tu n’as pas les droits nécessaires pour effectuer cette action.'
    }

    return error.message || 'Une erreur est survenue lors de la requête.'
  }

  return 'Une erreur inattendue est survenue.'
}

async function loadUsers() {
  loading.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const response = await api.get<User[]>('/users')
    users.value = response.data
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

async function addUser() {
  submitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await api.post('/users', { ...newUser })

    successMessage.value = 'Utilisateur ajouté avec succès.'

    newUser.firstName = ''
    newUser.lastName = ''
    newUser.email = ''
    newUser.userName = ''
    newUser.password = ''
    newUser.isAdmin = false

    await loadUsers()
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    submitting.value = false
  }
}

async function deleteUser(id: string) {
  if (!confirm('Es-tu sûr de vouloir supprimer cet utilisateur ?')) {
    return
  }

  deletingId.value = id
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await api.delete(`/users/${id}`)

    successMessage.value = 'Utilisateur supprimé avec succès.'
    await loadUsers()
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    deletingId.value = null
  }
}

onMounted(loadUsers)
</script>
