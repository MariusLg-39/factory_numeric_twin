<script setup lang="ts">
import { useHomePage } from './HomePageComponent'

const { user } = useHomePage()
</script>

<template>
  <div class="dashboard">
    <div class="welcome-section">
      <span class="welcome-label">
        FACTORY TWIN
      </span>

      <h2>
        Bienvenue
        <span v-if="user">
          {{ user.firstName }}
        </span>
        !
      </h2>

      <p>
        Retrouvez ici les informations principales
        de votre environnement industriel.
      </p>
    </div>

    <!-- Cards statistiques -->
    <div class="stats-grid">
      <PrimeCard class="stat-card">
        <template #content>
          <div class="stat-content">
            <div class="stat-icon lines">
              <i class="pi pi-sitemap" />
            </div>

            <div>
              <span class="stat-label">Lignes</span>
              <strong class="stat-value">0</strong>
            </div>
          </div>
        </template>
      </PrimeCard>

      <PrimeCard class="stat-card">
        <template #content>
          <div class="stat-content">
            <div class="stat-icon products">
              <i class="pi pi-box" />
            </div>

            <div>
              <span class="stat-label">Produits</span>
              <strong class="stat-value">0</strong>
            </div>
          </div>
        </template>
      </PrimeCard>

      <PrimeCard class="stat-card">
        <template #content>
          <div class="stat-content">
            <div class="stat-icon machines">
              <i class="pi pi-cog" />
            </div>

            <div>
              <span class="stat-label">Machines</span>
              <strong class="stat-value">0</strong>
            </div>
          </div>
        </template>
      </PrimeCard>

      <PrimeCard class="stat-card">
        <template #content>
          <div class="stat-content">
            <div class="stat-icon users">
              <i class="pi pi-users" />
            </div>

            <div>
              <span class="stat-label">Utilisateurs</span>
              <strong class="stat-value">0</strong>
            </div>
          </div>
        </template>
      </PrimeCard>
    </div>

    <!-- Zone principale -->
    <PrimeCard class="content-card">
      <template #title>
        Vue d'ensemble
      </template>

      <template #subtitle>
        Les données de votre usine apparaîtront ici.
      </template>

      <template #content>
        <div class="empty-state">
          <div class="empty-icon">
            <i class="pi pi-chart-line" />
          </div>

          <h3>
            Aucune donnée disponible
          </h3>

          <p>
            Commencez par configurer vos lignes,
            machines et produits.
          </p>
        </div>
      </template>
    </PrimeCard>
  </div>
</template>

<style scoped src="./HomePageComponent.css" />
