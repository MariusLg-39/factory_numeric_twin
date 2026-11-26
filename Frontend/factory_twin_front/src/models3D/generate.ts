import * as THREE from 'three'
import { GLTFExporter } from 'three/examples/jsm/exporters/GLTFExporter.js'
import { modelFileName, normalizeParams, type ModelKind } from './catalog'
import { buildRobotArm } from './robotArm'
import {
  buildAdjustableConveyor,
  buildCurveConveyor,
  buildInclineConveyor,
  buildStraightConveyor,
} from './conveyors'
import type { BuiltModel } from './types'
import { renderThumbnail } from './thumbnails'

// Construit le modèle 3D (objets three.js) correspondant au type demandé
export function buildModel(kind: ModelKind, rawParams: Record<string, number> = {}): BuiltModel {
  const p = normalizeParams(kind, rawParams)

  switch (kind) {
    case 'robot-arm':
      return buildRobotArm()
    case 'conveyor-straight':
      return buildStraightConveyor({ length: p.length!, width: p.width!, height: p.height! })
    case 'conveyor-incline':
      return buildInclineConveyor({
        length: p.length!,
        width: p.width!,
        heightLow: p.heightLow!,
        heightHigh: p.heightHigh!,
      })
    case 'conveyor-curve':
      return buildCurveConveyor({
        angle: p.angle!,
        innerRadius: p.innerRadius!,
        width: p.width!,
        height: p.height!,
      })
    case 'conveyor-adjustable':
      return buildAdjustableConveyor({ length: p.length!, width: p.width! })
  }
}

// Exporte en .glb binaire (géométrie, matériaux, texture de bande, animations, métadonnées)
export async function exportGlb(model: BuiltModel): Promise<ArrayBuffer> {
  const scene = new THREE.Scene()
  scene.add(model.root)
  scene.updateMatrixWorld(true)

  const result = await new GLTFExporter().parseAsync(scene, {
    binary: true,
    animations: model.animations,
  })

  scene.remove(model.root)
  return result as ArrayBuffer
}

export async function generateModelFile(kind: ModelKind, params: Record<string, number> = {}): Promise<File> {
  const normalized = normalizeParams(kind, params)
  const model = buildModel(kind, normalized)
  const buffer = await exportGlb(model)
  disposeModel(model)

  return new File([buffer], modelFileName(kind, normalized), { type: 'model/gltf-binary' })
}

// Aperçu (image) d'un modèle généré, sans passer par l'export .glb
export async function previewThumbnail(kind: ModelKind, params: Record<string, number> = {}): Promise<string> {
  const model = buildModel(kind, params)
  try {
    return await renderThumbnail(model.root)
  } finally {
    disposeModel(model)
  }
}

function disposeModel(model: BuiltModel) {
  model.root.traverse((child) => {
    const mesh = child as THREE.Mesh
    if (!mesh.isMesh) return
    mesh.geometry.dispose()
    const materials = Array.isArray(mesh.material) ? mesh.material : [mesh.material]
    for (const material of materials) {
      ;(material as THREE.MeshStandardMaterial).map?.dispose()
      material.dispose()
    }
  })
}
