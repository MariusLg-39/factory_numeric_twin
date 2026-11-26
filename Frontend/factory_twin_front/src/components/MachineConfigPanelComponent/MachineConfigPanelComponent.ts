import { reactive, ref, watch } from 'vue'
import {
  apiErrorMessage,
  createConfiguration,
  deleteConfiguration,
  getConfigurations,
  updateConfiguration,
  type MachineConfiguration,
  type MachineConfigurationRequest,
} from '@/services/machineService'

// Ligne éditable : copie locale d'une configuration + état d'édition
export interface EditableConfiguration extends MachineConfigurationRequest {
  id: string
  expanded: boolean
  saving: boolean
  original: MachineConfigurationRequest
}

function toRequest(c: MachineConfigurationRequest): MachineConfigurationRequest {
  return {
    name: c.name,
    value: c.value,
    unit: c.unit,
    minValue: c.minValue,
    maxValue: c.maxValue,
  }
}

function toEditable(c: MachineConfiguration): EditableConfiguration {
  const request = toRequest(c)
  return { ...request, id: c.id, expanded: false, saving: false, original: { ...request } }
}

function emptyRequest(): MachineConfigurationRequest {
  return { name: '', value: 0, unit: '', minValue: 0, maxValue: 100 }
}

// Prévient le parent à chaque changement (l'éditeur 3D applique « Hauteur » / « Vitesse »)
export type ConfigChangedEmit = (e: 'changed', configurations: MachineConfiguration[]) => void

export function useMachineConfigPanel(props: { machineId: string }, emit?: ConfigChangedEmit) {
  const configurations = ref<EditableConfiguration[]>([])
  const loading = ref(false)
  const errorMessage = ref('')
  const showAddForm = ref(false)
  const adding = ref(false)
  const newConfiguration = reactive<MachineConfigurationRequest>(emptyRequest())

  function notify() {
    emit?.(
      'changed',
      configurations.value.map((c) => ({
        ...c.original,
        id: c.id,
        machineId: props.machineId,
        createdAt: '',
        updatedAt: '',
      })),
    )
  }

  async function load() {
    loading.value = true
    errorMessage.value = ''

    try {
      const data = await getConfigurations(props.machineId)
      configurations.value = data.map(toEditable)
      notify()
    } catch (error) {
      errorMessage.value = apiErrorMessage(error, 'Impossible de charger les configurations.')
    } finally {
      loading.value = false
    }
  }

  function isDirty(c: EditableConfiguration): boolean {
    const o = c.original
    return (
      c.name !== o.name ||
      c.value !== o.value ||
      c.unit !== o.unit ||
      c.minValue !== o.minValue ||
      c.maxValue !== o.maxValue
    )
  }

  function validate(c: MachineConfigurationRequest): string | null {
    if (!c.name.trim()) return 'Le nom du paramètre est obligatoire.'
    if (c.minValue > c.maxValue) return 'Le minimum doit être inférieur ou égal au maximum.'
    if (c.value < c.minValue || c.value > c.maxValue)
      return `La valeur doit être comprise entre ${c.minValue} et ${c.maxValue}.`
    return null
  }

  async function save(c: EditableConfiguration) {
    const error = validate(c)
    if (error) {
      errorMessage.value = error
      return
    }

    c.saving = true
    errorMessage.value = ''

    try {
      const updated = await updateConfiguration(props.machineId, c.id, toRequest(c))
      Object.assign(c, toRequest(updated), { original: toRequest(updated), expanded: false })
      notify()
    } catch (err) {
      errorMessage.value = apiErrorMessage(err, "Impossible d'enregistrer la configuration.")
    } finally {
      c.saving = false
    }
  }

  function cancel(c: EditableConfiguration) {
    Object.assign(c, c.original, { expanded: false })
  }

  async function remove(c: EditableConfiguration) {
    if (!confirm(`Supprimer le paramètre « ${c.name} » ?`)) return

    errorMessage.value = ''

    try {
      await deleteConfiguration(props.machineId, c.id)
      configurations.value = configurations.value.filter((x) => x.id !== c.id)
      notify()
    } catch (err) {
      errorMessage.value = apiErrorMessage(err, 'Impossible de supprimer la configuration.')
    }
  }

  async function add() {
    const error = validate(newConfiguration)
    if (error) {
      errorMessage.value = error
      return
    }

    adding.value = true
    errorMessage.value = ''

    try {
      const created = await createConfiguration(props.machineId, toRequest(newConfiguration))
      configurations.value.push(toEditable(created))
      notify()
      Object.assign(newConfiguration, emptyRequest())
      showAddForm.value = false
    } catch (err) {
      errorMessage.value = apiErrorMessage(err, "Impossible d'ajouter la configuration.")
    } finally {
      adding.value = false
    }
  }

  // Recharge quand on change de machine (ex : clic sur une autre machine dans l'éditeur)
  watch(
    () => props.machineId,
    () => {
      showAddForm.value = false
      load()
    },
    { immediate: true },
  )

  return {
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
  }
}
