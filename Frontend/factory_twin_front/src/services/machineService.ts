import api from './api'

// ============================================================
// TYPES
// ============================================================

export type MachineStatus = 'Running' | 'Stopped' | 'Maintenance' | 'Error'

export const MACHINE_STATUSES: { value: MachineStatus; label: string; severity: string }[] = [
  { value: 'Running', label: 'En marche', severity: 'success' },
  { value: 'Stopped', label: 'Arrêtée', severity: 'secondary' },
  { value: 'Maintenance', label: 'Maintenance', severity: 'warn' },
  { value: 'Error', label: 'En panne', severity: 'danger' },
]

export function statusLabel(status: string): string {
  return MACHINE_STATUSES.find((s) => s.value === status)?.label ?? status
}

export function statusSeverity(status: string): string {
  return MACHINE_STATUSES.find((s) => s.value === status)?.severity ?? 'secondary'
}

// Machine réelle d'une ligne de production (instance d'un modèle du catalogue)
export interface Machine {
  id: string
  name: string
  status: MachineStatus
  productionLineId: string
  productionLineName: string | null
  machineModelId: string
  machineModelName: string | null
  type: string // type du modèle
  hasModel3D: boolean
  model3DFileName: string | null
  positionX: number
  positionY: number
  positionZ: number
  rotationY: number
  scale: number
  createdAt: string
  updatedAt: string
}

export interface MachinePlacement {
  positionX: number
  positionY: number
  positionZ: number
  rotationY: number
  scale: number
}

export interface CreateMachineRequest {
  machineModelId: string
  productionLineId: string
  name?: string // vide : nom automatique (« Bras robot 6 axes 2 »)
  status?: MachineStatus
  placement?: MachinePlacement
}

export interface MachineRequest {
  name: string
  status: MachineStatus
  productionLineId: string
}

export interface MachineConfiguration {
  id: string
  machineId: string
  name: string
  value: number
  unit: string
  minValue: number
  maxValue: number
  createdAt: string
  updatedAt: string
}

export interface MachineConfigurationRequest {
  name: string
  value: number
  unit: string
  minValue: number
  maxValue: number
}

// Ligne de production (regroupement de machines dans l'usine)
export interface ProductionLine {
  id: string
  name: string
  description: string
  machineCount: number
  createdAt: string
  updatedAt: string
}

export type ProductionLineOption = Pick<ProductionLine, 'id' | 'name'>

export interface ProductionLineRequest {
  name: string
  description: string
}

// ============================================================
// MACHINES
// ============================================================

export async function getMachines(productionLineId?: string | null): Promise<Machine[]> {
  const { data } = await api.get<Machine[]>('/machines', {
    params: productionLineId ? { productionLineId } : undefined,
  })
  return data
}

export async function createMachine(request: CreateMachineRequest): Promise<Machine> {
  const { data } = await api.post<Machine>('/machines', request)
  return data
}

// Copie une machine et ses configurations (posée à côté de l'originale)
export async function duplicateMachine(id: string): Promise<Machine> {
  const { data } = await api.post<Machine>(`/machines/${id}/duplicate`)
  return data
}

export async function updateMachine(id: string, request: MachineRequest): Promise<void> {
  await api.put(`/machines/${id}`, request)
}

export async function deleteMachine(id: string): Promise<void> {
  await api.delete(`/machines/${id}`)
}

export async function updatePlacement(id: string, placement: MachinePlacement): Promise<Machine> {
  const { data } = await api.put<Machine>(`/machines/${id}/placement`, placement)
  return data
}

// ============================================================
// CONFIGURATIONS D'UNE MACHINE
// ============================================================

export async function getConfigurations(machineId: string): Promise<MachineConfiguration[]> {
  const { data } = await api.get<MachineConfiguration[]>(`/machines/${machineId}/configurations`)
  return data
}

export async function createConfiguration(
  machineId: string,
  request: MachineConfigurationRequest,
): Promise<MachineConfiguration> {
  const { data } = await api.post<MachineConfiguration>(`/machines/${machineId}/configurations`, request)
  return data
}

export async function updateConfiguration(
  machineId: string,
  configurationId: string,
  request: MachineConfigurationRequest,
): Promise<MachineConfiguration> {
  const { data } = await api.put<MachineConfiguration>(
    `/machines/${machineId}/configurations/${configurationId}`,
    request,
  )
  return data
}

export async function deleteConfiguration(machineId: string, configurationId: string): Promise<void> {
  await api.delete(`/machines/${machineId}/configurations/${configurationId}`)
}

// ============================================================
// LIGNES DE PRODUCTION (jumeaux numériques)
// ============================================================

export async function getProductionLines(): Promise<ProductionLine[]> {
  const { data } = await api.get<ProductionLine[]>('/productionLine')
  return data
}

export async function createProductionLine(request: ProductionLineRequest): Promise<ProductionLine> {
  const { data } = await api.post<ProductionLine>('/productionLine', request)
  return data
}

export async function updateProductionLine(id: string, request: ProductionLineRequest): Promise<void> {
  await api.put(`/productionLine/${id}`, request)
}

export async function deleteProductionLine(id: string): Promise<void> {
  await api.delete(`/productionLine/${id}`)
}

// Extrait un message lisible d'une erreur axios renvoyée par l'API
export function apiErrorMessage(error: unknown, fallback: string): string {
  const err = error as {
    response?: { data?: { message?: string; title?: string; errors?: Record<string, string[]> } }
  }
  const data = err.response?.data

  if (data?.message) return data.message

  if (data?.errors) {
    const first = Object.values(data.errors)[0]
    if (first?.[0]) return first[0]
  }

  return data?.title ?? fallback
}
