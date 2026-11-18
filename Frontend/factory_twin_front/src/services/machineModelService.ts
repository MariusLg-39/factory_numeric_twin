import api from './api'
import type { MachineConfigurationRequest } from './machineService'

// ============================================================
// Catalogue des modèles de machines de l'usine
// ============================================================

export interface DefaultConfiguration extends MachineConfigurationRequest {
  id: string
}

export interface MachineModel {
  id: string
  name: string
  type: string
  description: string
  hasModel3D: boolean
  model3DFileName: string | null
  defaultConfigurations: DefaultConfiguration[]
  machineCount: number // machines créées à partir de ce modèle (toutes lignes)
  createdAt: string
  updatedAt: string
}

export interface MachineModelRequest {
  name: string
  type: string
  description: string
  defaultConfigurations: MachineConfigurationRequest[]
}

export async function getMachineModels(): Promise<MachineModel[]> {
  const { data } = await api.get<MachineModel[]>('/machine-models')
  return data
}

export async function createMachineModel(request: MachineModelRequest): Promise<MachineModel> {
  const { data } = await api.post<MachineModel>('/machine-models', request)
  return data
}

export async function updateMachineModel(id: string, request: MachineModelRequest): Promise<MachineModel> {
  const { data } = await api.put<MachineModel>(`/machine-models/${id}`, request)
  return data
}

export async function deleteMachineModel(id: string): Promise<void> {
  await api.delete(`/machine-models/${id}`)
}

// ---------------- Modèle 3D (.glb) ----------------

export async function uploadModel3D(id: string, file: File): Promise<MachineModel> {
  const form = new FormData()
  form.append('file', file)

  const { data } = await api.post<MachineModel>(`/machine-models/${id}/model`, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

// Le fichier est récupéré via axios pour profiter du token JWT
export async function downloadModel3D(id: string): Promise<ArrayBuffer> {
  const { data } = await api.get<ArrayBuffer>(`/machine-models/${id}/model`, {
    responseType: 'arraybuffer',
  })
  return data
}

export async function deleteModel3D(id: string): Promise<void> {
  await api.delete(`/machine-models/${id}/model`)
}

// Clé de cache des vignettes : change quand le modèle est modifié
export function thumbnailKey(model: Pick<MachineModel, 'id' | 'updatedAt' | 'model3DFileName'>): string {
  return `model:${model.id}:${model.updatedAt}:${model.model3DFileName}`
}
