import { computed, onMounted, reactive, ref, watch } from 'vue'
import { apiErrorMessage, type MachineConfigurationRequest } from '@/services/machineService'
import {
  createMachineModel,
  deleteMachineModel,
  deleteModel3D,
  downloadModel3D,
  getMachineModels,
  thumbnailKey,
  updateMachineModel,
  uploadModel3D,
  type MachineModel,
  type MachineModelRequest,
} from '@/services/machineModelService'
import {
  MODEL_TYPES,
  defaultParams,
  getModelType,
  guessModelKind,
  suggestedConfigurations,
  type ModelKind,
} from '@/models3D/catalog'

const MAX_MODEL_SIZE = 100 * 1024 * 1024

// Que faire du modèle 3D à l'enregistrement
export type ModelMode = 'keep' | 'none' | 'upload' | 'generate'

interface ModelForm {
  name: string
  type: string
  description: string
  defaultConfigurations: MachineConfigurationRequest[]
}

function emptyForm(): ModelForm {
  return { name: '', type: '', description: '', defaultConfigurations: [] }
}

export function useCatalogPage() {
  const models = ref<MachineModel[]>([])
  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')
  const search = ref('')
  const thumbnails = ref<Record<string, string>>({})

  const filteredModels = computed(() => {
    const s = search.value.trim().toLowerCase()
    return models.value.filter((m) => !s || `${m.name} ${m.type} ${m.description}`.toLowerCase().includes(s))
  })

  // ---------------- Formulaire ----------------
  const formVisible = ref(false)
  const editing = ref<MachineModel | null>(null)
  const form = reactive<ModelForm>(emptyForm())
  const formError = ref('')
  const saving = ref(false)

  const formTitle = computed(() => (editing.value ? 'Modifier le modèle' : 'Nouveau modèle de machine'))

  // ---------------- Modèle 3D ----------------
  const modelMode = ref<ModelMode>('none')
  const modelFile = ref<File | null>(null)
  const fileInput = ref<HTMLInputElement | null>(null)
  const generateKind = ref<ModelKind>('robot-arm')
  const generateParams = reactive<Record<string, number>>({})
  const generateType = computed(() => getModelType(generateKind.value))

  const modelModeOptions = computed(() => {
    const options: { value: ModelMode; label: string; icon: string }[] = []
    if (editing.value?.hasModel3D) options.push({ value: 'keep', label: 'Conserver', icon: 'pi pi-check' })
    options.push(
      { value: 'upload', label: 'Importer', icon: 'pi pi-upload' },
      { value: 'generate', label: 'Générer', icon: 'pi pi-wrench' },
      { value: 'none', label: 'Aucun', icon: 'pi pi-ban' },
    )
    return options
  })

  const previewUrl = ref<string | null>(null)
  const previewLoading = ref(false)
  const previewError = ref('')
  let previewToken = 0
  let previewTimer: ReturnType<typeof setTimeout> | undefined

  function resetGenerateParams(kind: ModelKind) {
    for (const key of Object.keys(generateParams)) delete generateParams[key]
    Object.assign(generateParams, defaultParams(kind))
  }

  watch(generateKind, (kind) => {
    resetGenerateParams(kind)
    applySuggestions(kind)
  })

  watch(modelMode, (mode, previous) => {
    // En passant sur « Générer », propose le type le plus probable d'après la saisie
    if (mode === 'generate' && previous !== 'generate' && `${form.type}${form.name}`.trim()) {
      const kind = guessModelKind(`${form.type} ${form.name}`)
      if (kind === generateKind.value) {
        resetGenerateParams(kind)
        applySuggestions(kind)
      }
      generateKind.value = kind
    }
  })

  // Pré-remplit les configurations par défaut si l'utilisateur n'en a pas encore saisi
  let suggested = false
  let suggestedType = ''
  function applySuggestions(kind: ModelKind) {
    if (modelMode.value !== 'generate') return

    const label = getModelType(kind).label
    if (!form.type || form.type === suggestedType) {
      form.type = label
      suggestedType = label
    }

    if (form.defaultConfigurations.length > 0 && !suggested) return
    form.defaultConfigurations = suggestedConfigurations(kind)
    suggested = true
  }

  function schedulePreview() {
    clearTimeout(previewTimer)
    previewTimer = setTimeout(updatePreview, 250)
  }

  async function updatePreview() {
    const token = ++previewToken
    previewError.value = ''

    if (!formVisible.value || modelMode.value === 'none' || (modelMode.value === 'upload' && !modelFile.value)) {
      previewUrl.value = null
      previewLoading.value = false
      return
    }

    previewLoading.value = true
    try {
      const thumbnailsModule = await import('@/models3d/thumbnails')
      let url: string

      if (modelMode.value === 'generate') {
        const { previewThumbnail } = await import('@/models3d/generate')
        url = await previewThumbnail(generateKind.value, { ...generateParams })
      } else if (modelMode.value === 'upload') {
        url = await thumbnailsModule.renderThumbnail(await modelFile.value!.arrayBuffer())
      } else {
        const model = editing.value!
        url = await thumbnailsModule.cachedThumbnail(thumbnailKey(model), () => downloadModel3D(model.id))
      }

      if (token === previewToken) previewUrl.value = url
    } catch {
      if (token === previewToken) {
        previewUrl.value = null
        previewError.value = 'Aperçu impossible : le fichier est peut-être invalide.'
      }
    } finally {
      if (token === previewToken) previewLoading.value = false
    }
  }

  watch([formVisible, modelMode, modelFile, generateKind, () => ({ ...generateParams })], schedulePreview, {
    deep: true,
  })

  function pickFile() {
    fileInput.value?.click()
  }

  function onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement
    const file = input.files?.[0]
    input.value = ''
    if (!file) return

    if (!file.name.toLowerCase().endsWith('.glb')) {
      formError.value = 'Le modèle doit être un fichier .glb.'
      return
    }
    if (file.size > MAX_MODEL_SIZE) {
      formError.value = 'Le fichier dépasse 100 Mo.'
      return
    }

    formError.value = ''
    modelFile.value = file
  }

  // ---------------- Configurations par défaut ----------------

  function addConfiguration() {
    form.defaultConfigurations.push({ name: '', value: 0, unit: '', minValue: 0, maxValue: 100 })
    suggested = false
  }

  function removeConfiguration(index: number) {
    form.defaultConfigurations.splice(index, 1)
    suggested = false
  }

  function validateConfigurations(): string | null {
    for (const c of form.defaultConfigurations) {
      if (!c.name.trim()) return 'Chaque paramètre doit avoir un nom.'
      if (c.minValue > c.maxValue) return `« ${c.name} » : le minimum doit être inférieur ou égal au maximum.`
      if (c.value < c.minValue || c.value > c.maxValue)
        return `« ${c.name} » : la valeur doit être comprise entre ${c.minValue} et ${c.maxValue}.`
    }
    return null
  }

  // ---------------- Chargement / CRUD ----------------

  function flash(message: string) {
    successMessage.value = message
    setTimeout(() => {
      if (successMessage.value === message) successMessage.value = ''
    }, 3500)
  }

  async function loadThumbnails(list: MachineModel[]) {
    const pending = list.filter((m) => m.hasModel3D)
    if (pending.length === 0) return

    const { cachedThumbnail } = await import('@/models3d/thumbnails')
    for (const model of pending) {
      cachedThumbnail(thumbnailKey(model), () => downloadModel3D(model.id))
        .then((url) => (thumbnails.value[model.id] = url))
        .catch(() => undefined)
    }
  }

  async function load() {
    loading.value = true
    errorMessage.value = ''

    try {
      models.value = await getMachineModels()
      thumbnails.value = {}
      loadThumbnails(models.value)
    } catch (error) {
      errorMessage.value = apiErrorMessage(error, 'Impossible de charger le catalogue.')
    } finally {
      loading.value = false
    }
  }

  function openForm(model: MachineModel | null) {
    editing.value = model
    formError.value = ''
    modelFile.value = null
    previewUrl.value = null
    suggested = false
    suggestedType = ''

    if (model) {
      Object.assign(form, {
        name: model.name,
        type: model.type,
        description: model.description,
        defaultConfigurations: model.defaultConfigurations.map((c) => ({
          name: c.name,
          value: c.value,
          unit: c.unit,
          minValue: c.minValue,
          maxValue: c.maxValue,
        })),
      })
      modelMode.value = model.hasModel3D ? 'keep' : 'none'
    } else {
      Object.assign(form, emptyForm())
      modelMode.value = 'generate'
      // Le watch de modelMode ne se déclenche pas si la valeur ne change pas
      applySuggestions(generateKind.value)
    }

    formVisible.value = true
  }

  async function submit() {
    if (modelMode.value === 'upload' && !modelFile.value) {
      formError.value = 'Choisis un fichier .glb à importer.'
      return
    }
    const configError = validateConfigurations()
    if (configError) {
      formError.value = configError
      return
    }

    saving.value = true
    formError.value = ''
    const wasEditing = editing.value

    const request: MachineModelRequest = {
      name: form.name.trim(),
      type: form.type.trim(),
      description: form.description.trim(),
      defaultConfigurations: form.defaultConfigurations.map((c) => ({ ...c, name: c.name.trim() })),
    }

    // 1. Le modèle
    let model: MachineModel
    try {
      model = wasEditing ? await updateMachineModel(wasEditing.id, request) : await createMachineModel(request)
    } catch (error) {
      formError.value = apiErrorMessage(error, "Impossible d'enregistrer le modèle.")
      saving.value = false
      return
    }

    // 2. Son fichier 3D
    try {
      if (modelMode.value === 'upload' && modelFile.value) {
        await uploadModel3D(model.id, modelFile.value)
      } else if (modelMode.value === 'generate') {
        const { generateModelFile } = await import('@/models3d/generate')
        await uploadModel3D(model.id, await generateModelFile(generateKind.value, { ...generateParams }))
      } else if (modelMode.value === 'none' && wasEditing?.hasModel3D) {
        await deleteModel3D(model.id)
      }
      flash(wasEditing ? 'Modèle modifié.' : 'Modèle ajouté au catalogue. Pose-le dans l\'usine depuis l\'éditeur 3D.')
    } catch (error) {
      errorMessage.value = `Modèle enregistré, mais le fichier 3D n'a pas pu être envoyé : ${apiErrorMessage(error, 'erreur inconnue')}`
    }

    saving.value = false
    formVisible.value = false
    await load()
  }

  async function remove(model: MachineModel) {
    if (!confirm(`Supprimer le modèle « ${model.name} » du catalogue ?`)) return

    errorMessage.value = ''
    try {
      await deleteMachineModel(model.id)
      models.value = models.value.filter((m) => m.id !== model.id)
      flash('Modèle supprimé.')
    } catch (error) {
      errorMessage.value = apiErrorMessage(error, 'Impossible de supprimer le modèle.')
    }
  }

  onMounted(load)

  return {
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
    modelTypes: MODEL_TYPES,
    generateKind,
    generateParams,
    generateType,
    previewUrl,
    previewLoading,
    previewError,
    openCreate: () => openForm(null),
    openEdit: (model: MachineModel) => openForm(model),
    submit,
    remove,
    pickFile,
    onFileSelected,
    addConfiguration,
    removeConfiguration,
  }
}
