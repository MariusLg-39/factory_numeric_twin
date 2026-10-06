<template>
  <div class="product-page">

    <h1>Produit à fabriquer</h1>

    <div class="product-card">

      <div class="product-header">
        <div>
          <h2>{{ product.name }}</h2>
          <p class="reference">
            Référence : {{ product.reference }}
          </p>
        </div>

        <span class="status">
          {{ product.status }}
        </span>
      </div>

      <p class="description">
        {{ product.description }}
      </p>

      <div class="product-info">

        <div class="info">
          <span class="label">Capacité</span>
          <span class="value">{{ product.capacity }}</span>
        </div>

        <div class="info">
          <span class="label">Temps de production</span>
          <span class="value">{{ product.productionTime }} min</span>
        </div>

        <div class="info">
          <span class="label">Quantité</span>
          <span class="value">{{ product.quantity }} unités</span>
        </div>

      </div>

      <div class="section">

        <h3>Recette de fabrication</h3>

        <div
          v-for="step in product.recipe"
          :key="step.id"
          class="recipe-step"
        >
          <span class="step-number">
            {{ step.id }}
          </span>

          <div>
            <strong>{{ step.name }}</strong>

            <p>
              {{ step.description }}
            </p>
          </div>
        </div>

      </div>

      <div class="section">

        <h3>Machines nécessaires</h3>

        <div class="machines">

          <div
            v-for="machine in product.machines"
            :key="machine.id"
            class="machine"
          >

            <span class="machine-icon">
              ⚙️
            </span>

            <div>
              <strong>{{ machine.name }}</strong>

              <p>
                {{ machine.description }}
              </p>
            </div>

          </div>

        </div>

      </div>

      <button
        class="select-button"
        @click="selectProduct"
      >
        Sélectionner ce produit
      </button>

    </div>

  </div>
</template>

<script lang="ts" src="./ProductComponent.ts"></script>

<style scoped src="./ProductComponent.css"></style>