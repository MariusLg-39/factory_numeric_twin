import type * as THREE from 'three'

// Modèle construit : la racine (à exporter en .glb) + ses clips d'animation
export interface BuiltModel {
  root: THREE.Object3D
  animations: THREE.AnimationClip[]
}

// ============================================================
// Métadonnées enregistrées dans le .glb (extras glTF du nœud racine,
// lues par machineRigs.ts pour animer le modèle dans l'éditeur)
// ============================================================

export interface HeightRigMeta {
  min: number
  max: number
  default: number
  bodyNode: string // nœud déplacé verticalement (le plateau du convoyeur)
  legPrefix: string // nœuds étirés verticalement (tubes intérieurs des pieds)
  legBase: number // hauteur (m) où commencent les tubes intérieurs
  topOffset: number // décalage entre le dessus de la bande et le haut des pieds
}

export interface RigMeta {
  kind: 'robot-arm' | 'conveyor'
  variant?: 'straight' | 'incline' | 'curve' | 'adjustable'
  version: number
  clip?: string // nom du clip d'animation principal
  beltMaterial?: string // matériau de la bande (texture défilante)
  uvPerMeter?: number // répétitions de texture par mètre de bande
  speed?: number // vitesse par défaut de la bande (m/s)
  height?: HeightRigMeta
}

export const RIG_KEY = 'factoryTwin'
