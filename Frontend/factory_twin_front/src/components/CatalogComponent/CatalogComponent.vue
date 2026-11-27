<script setup lang="ts">
import { useCatalogPage } from './CatalogComponent'

const {
  models,
  filteredModels,
  loading,
  errorMessage,
  successMessage,
  search,
  thumbnails,
  formVisible,
  formTitle,
  form,
  formError,
  saving,
  editing,
  modelMode,
  modelModeOptions,
  modelFile,
  fileInput,
  modelTypes,
  generateKind,
  generateParams,
  generateType,
  previewUrl,
  previewLoading,
  previewError,
  openCreate,
  openEdit,
  submit,
  remove,
  pickFile,
  onFileSelected,
  addConfiguration,
  removeConfiguration,
} = useCatalogPage()
</script>

<template>
  <div class="catalog-page">
    <div class="page-header">
      <div>
        <p class="intro">
          Les modèles de machines de l'usine. Dans l'éditeur 3D, chaque modèle peut être posé
          autant de fois que nécessaire : chaque machine garde ses propres configurations.
        </p>
      </div>
      <div class="header-actions">
        <span class="search">
          <i class="pi pi-search" />
          <PrimeInputText v-model="search" placeholder="Rechercher…" size="small" />
        </span>
        <PrimeButton label="Nouveau modèle" icon="pi pi-plus" @click="openCreate" />
      </div>
    </div>

    <PrimeMessage v-if="errorMessage" severity="error" closable @close="errorMessage = ''">
      {{ errorMessage }}
    </PrimeMessage>
    <PrimeMessage v-if="successMessage" severity="success" :closable="false">
      {{ successMessage }}
    </PrimeMessage>

    <p v-if="loading" class="muted">Chargement du catalogue…</p>

    <div v-else-if="models.length === 0" class="empty">
      <i class="pi pi-box" />
      <strong>Le catalogue est vide</strong>
      <span>Crée un premier modèle : un bras robot ou un convoyeur peuvent être générés automatiquement.</span>
      <PrimeButton label="Nouveau modèle" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-else class="model-grid">
      <article v-for="m in filteredModels" :key="m.id" class="model-card">
        <button type="button" class="model-thumb" @click="openEdit(m)">
          <img v-if="thumbnails[m.id]" :src="thumbnails[m.id]" :alt="m.name" />
          <i v-else-if="m.hasModel3D" class="pi pi-spin pi-spinner" />
          <span v-else class="no-model"><i class="pi pi-box" /> Sans modèle 3D</span>
        </button>

        <div class="model-body">
          <div class="model-title">
            <strong>{{ m.name }}</strong>
            <span>{{ m.type || 'Sans type' }}</span>
          </div>
          <p v-if="m.description" class="model-description">{{ m.description }}</p>

          <div class="chips">
            <span class="chip"><i class="pi pi-sitemap" /> {{ m.machineCount }} machine(s)</span>
            <span class="chip"><i class="pi pi-sliders-h" /> {{ m.defaultConfigurations.length }} paramètre(s)</span>
          </div>
        </div>

        <div class="model-actions">
          <PrimeButton label="Modifier" icon="pi pi-pencil" text size="small" @click="openEdit(m)" />
          <PrimeButton
            icon="pi pi-trash"
            text
            rounded
            size="small"
            severity="danger"
            aria-label="Supprimer"
            v-tooltip.top="m.machineCount ? 'Utilisé par des machines' : 'Supprimer'"
            :disabled="m.machineCount > 0"
            @click="remove(m)"
          />
        </div>
      </article>
    </div>

    <!-- Ajout / modification d'un modèle -->
    <PrimeDialog
      v-model:visible="formVisible"
      :header="formTitle"
      modal
      :closable="!saving"
      :style="{ width: '60rem' }"
      :breakpoints="{ '960px': '95vw' }"
    >
      <form class="model-form" @submit.prevent="submit">
        <div class="form-columns">
          <!-- Informations + configurations par défaut -->
          <div class="form-column">
            <label>
              Nom
              <PrimeInputText v-model="form.name" required maxlength="100" placeholder="ex : Robot KR 10" fluid />
            </label>

            <label>
              Type
              <PrimeInputText v-model="form.type" maxlength="100" placeholder="ex : Bras robot 6 axes" fluid />
            </label>

            <label>
              Description
              <PrimeInputText v-model="form.description" maxlength="500" placeholder="Facultatif" fluid />
            </label>

            <div class="section">
              <div class="section-header">
                <span class="section-label">Configurations par défaut</span>
                <PrimeButton label="Ajouter" icon="pi pi-plus" text size="small" @click="addConfiguration" />
              </div>
              <p class="muted small">
                Copiées dans chaque machine créée à partir de ce modèle, qui peut ensuite les modifier.
              </p>

              <div v-if="form.defaultConfigurations.length" class="config-table">
                <div class="config-row config-head">
                  <span>Paramètre</span>
                  <span>Valeur</span>
                  <span>Unité</span>
                  <span>Min</span>
                  <span>Max</span>
                  <span />
                </div>
                <div v-for="(c, index) in form.defaultConfigurations" :key="index" class="config-row">
                  <PrimeInputText v-model="c.name" maxlength="100" placeholder="Nom" size="small" />
                  <PrimeInputNumber v-model="c.value" :max-fraction-digits="3" size="small" fluid />
                  <PrimeInputText v-model="c.unit" maxlength="50" placeholder="—" size="small" />
                  <PrimeInputNumber v-model="c.minValue" :max-fraction-digits="3" size="small" fluid />
                  <PrimeInputNumber v-model="c.maxValue" :max-fraction-digits="3" size="small" fluid />
                  <PrimeButton
                    icon="pi pi-times"
                    text
                    rounded
                    size="small"
                    severity="secondary"
                    aria-label="Retirer"
                    @click="removeConfiguration(index)"
                  />
                </div>
              </div>
              <p v-else class="muted small">Aucun paramètre.</p>
            </div>
          </div>

          <!-- Modèle 3D -->
          <div class="form-column model-column">
            <span class="section-label">Modèle 3D</span>

            <PrimeSelectButton
              v-model="modelMode"
              :options="modelModeOptions"
              option-label="label"
              option-value="value"
              :allow-empty="false"
              class="mode-switch"
            >
              <template #option="{ option }">
                <i :class="option.icon" />
                <span>{{ option.label }}</span>
              </template>
            </PrimeSelectButton>

            <div v-if="modelMode === 'keep'" class="mode-body">
              <p class="muted"><i class="pi pi-box" /> {{ editing?.model3DFileName }}</p>
            </div>

            <div v-else-if="modelMode === 'upload'" class="mode-body">
              <input
                ref="fileInput"
                type="file"
                accept=".glb,model/gltf-binary"
                class="hidden-input"
                @change="onFileSelected"
              />
              <PrimeButton
                :label="modelFile ? 'Changer de fichier' : 'Choisir un fichier .glb'"
                icon="pi pi-folder-open"
                outlined
                size="small"
                @click="pickFile"
              />
              <p class="muted file-name">{{ modelFile ? modelFile.name : '100 Mo maximum' }}</p>
            </div>

            <div v-else-if="modelMode === 'generate'" class="mode-body">
              <PrimeSelect
                v-model="generateKind"
                :options="modelTypes"
                option-label="label"
                option-value="kind"
                size="small"
                fluid
              />
              <p class="muted">{{ generateType.description }}</p>
              <div v-if="generateType.params.length" class="param-grid">
                <label v-for="param in generateType.params" :key="`${generateKind}-${param.key}`">
                  {{ param.label }}
                  <PrimeInputNumber
                    v-model="generateParams[param.key]"
                    :min="param.min"
                    :max="param.max"
                    :step="param.step"
                    :max-fraction-digits="2"
                    :suffix="` ${param.unit}`"
                    show-buttons
                    size="small"
                    fluid
                  />
                </label>
              </div>
            </div>

            <div v-else class="mode-body">
              <p class="muted">Les machines apparaîtront comme une boîte dans l'éditeur 3D.</p>
            </div>

            <div v-if="modelMode !== 'none'" class="preview">
              <img v-if="previewUrl" :src="previewUrl" alt="Aperçu du modèle 3D" />
              <span v-else-if="previewLoading" class="preview-state"><i class="pi pi-spin pi-spinner" /> Aperçu…</span>
              <span v-else-if="previewError" class="preview-state error">{{ previewError }}</span>
              <span v-else class="preview-state">Aperçu</span>
              <i v-if="previewLoading && previewUrl" class="pi pi-spin pi-spinner preview-spinner" />
            </div>
          </div>
        </div>

        <PrimeMessage v-if="formError" severity="error" :closable="false">{{ formError }}</PrimeMessage>

        <div class="form-footer">
          <PrimeButton label="Annuler" text severity="secondary" :disabled="saving" @click="formVisible = false" />
          <PrimeButton type="submit" label="Enregistrer" :loading="saving" />
        </div>
      </form>
    </PrimeDialog>
  </div>
</template>

<style scoped src="./CatalogComponent.css" />
