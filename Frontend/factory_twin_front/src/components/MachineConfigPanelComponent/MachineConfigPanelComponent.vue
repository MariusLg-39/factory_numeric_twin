<script setup lang="ts">
import { useMachineConfigPanel } from './MachineConfigPanelComponent'
import type { MachineConfiguration } from '@/services/machineService'

const props = defineProps<{
  machineId: string
}>()

const emit = defineEmits<{
  (e: 'changed', configurations: MachineConfiguration[]): void
}>()

const {
  configurations,
  loading,
  errorMessage,
  showAddForm,
  adding,
  newConfiguration,
  isDirty,
  save,
  cancel,
  remove,
  add,
} = useMachineConfigPanel(props, emit)
</script>

<template>
  <div class="config-panel">
    <PrimeMessage v-if="errorMessage" severity="error" :closable="false">
      {{ errorMessage }}
    </PrimeMessage>

    <p v-if="loading" class="muted">Chargement des configurations...</p>

    <p v-else-if="configurations.length === 0 && !showAddForm" class="muted">
      Aucun paramètre pour cette machine.
    </p>

    <div v-for="c in configurations" :key="c.id" class="config-card">
      <div class="config-header">
        <strong class="config-name">{{ c.name }}</strong>

        <div class="config-actions">
          <PrimeButton
            icon="pi pi-pencil"
            text
            rounded
            size="small"
            aria-label="Modifier le paramètre"
            @click="c.expanded = !c.expanded"
          />
          <PrimeButton
            icon="pi pi-trash"
            text
            rounded
            size="small"
            severity="danger"
            aria-label="Supprimer le paramètre"
            @click="remove(c)"
          />
        </div>
      </div>

      <div class="config-value">
        <PrimeInputNumber
          v-model="c.value"
          :min="c.minValue"
          :max="c.maxValue"
          :max-fraction-digits="3"
          show-buttons
          button-layout="horizontal"
          :step="(c.maxValue - c.minValue) / 100 || 1"
          fluid
          :suffix="c.unit ? ` ${c.unit}` : ''"
        />
        <span class="range">{{ c.minValue }} – {{ c.maxValue }} {{ c.unit }}</span>
      </div>

      <div v-if="c.expanded" class="config-details">
        <label>
          Nom
          <PrimeInputText v-model="c.name" maxlength="100" fluid />
        </label>
        <div class="row">
          <label>
            Min
            <PrimeInputNumber v-model="c.minValue" :max-fraction-digits="3" fluid />
          </label>
          <label>
            Max
            <PrimeInputNumber v-model="c.maxValue" :max-fraction-digits="3" fluid />
          </label>
          <label>
            Unité
            <PrimeInputText v-model="c.unit" maxlength="50" fluid />
          </label>
        </div>
      </div>

      <div v-if="isDirty(c)" class="config-footer">
        <PrimeButton label="Annuler" text size="small" severity="secondary" @click="cancel(c)" />
        <PrimeButton label="Enregistrer" size="small" :loading="c.saving" @click="save(c)" />
      </div>
    </div>

    <form v-if="showAddForm" class="config-card add-form" @submit.prevent="add">
      <label>
        Nom du paramètre
        <PrimeInputText
          v-model="newConfiguration.name"
          maxlength="100"
          placeholder="ex : Vitesse de l'axe 1"
          fluid
          required
        />
      </label>
      <div class="row">
        <label>
          Min
          <PrimeInputNumber v-model="newConfiguration.minValue" :max-fraction-digits="3" fluid />
        </label>
        <label>
          Max
          <PrimeInputNumber v-model="newConfiguration.maxValue" :max-fraction-digits="3" fluid />
        </label>
      </div>
      <div class="row">
        <label>
          Valeur
          <PrimeInputNumber v-model="newConfiguration.value" :max-fraction-digits="3" fluid />
        </label>
        <label>
          Unité
          <PrimeInputText v-model="newConfiguration.unit" maxlength="50" placeholder="°/s" fluid />
        </label>
      </div>
      <div class="config-footer">
        <PrimeButton label="Annuler" text size="small" severity="secondary" @click="showAddForm = false" />
        <PrimeButton type="submit" label="Ajouter" size="small" :loading="adding" />
      </div>
    </form>

    <PrimeButton
      v-if="!showAddForm"
      label="Ajouter un paramètre"
      icon="pi pi-plus"
      outlined
      size="small"
      class="add-button"
      @click="showAddForm = true"
    />
  </div>
</template>

<style scoped src="./MachineConfigPanelComponent.css" />
