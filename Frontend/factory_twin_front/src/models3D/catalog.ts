// Catalogue des modèles générables. Fichier léger (sans three.js) :
// la génération elle-même est dans generate.ts, chargé à la demande.

export type ModelKind =
  | 'robot-arm'
  | 'conveyor-straight'
  | 'conveyor-incline'
  | 'conveyor-curve'
  | 'conveyor-adjustable'

export interface ModelParam {
  key: string
  label: string
  unit: string
  min: number
  max: number
  step: number
  default: number
}

export interface ModelType {
  kind: ModelKind
  label: string
  description: string
  params: ModelParam[]
}

const length = (value: number, min = 0.6): ModelParam => ({
  key: 'length',
  label: 'Longueur',
  unit: 'm',
  min,
  max: 8,
  step: 0.1,
  default: value,
})

const width: ModelParam = { key: 'width', label: 'Largeur de bande', unit: 'm', min: 0.2, max: 1.2, step: 0.05, default: 0.5 }
const height: ModelParam = { key: 'height', label: 'Hauteur de bande', unit: 'm', min: 0.4, max: 1.5, step: 0.05, default: 0.85 }

export const MODEL_TYPES: ModelType[] = [
  {
    kind: 'robot-arm',
    label: 'Bras robot 6 axes',
    description: 'Robot industriel articulé (portée ~1,4 m) avec pince. Animation de prise-dépose incluse.',
    params: [],
  },
  {
    kind: 'conveyor-straight',
    label: 'Convoyeur droit',
    description: 'Convoyeur à bande horizontal sur pieds fixes.',
    params: [length(2), width, height],
  },
  {
    kind: 'conveyor-incline',
    label: 'Convoyeur incliné',
    description: 'Monte de la hauteur basse vers la haute. Tourne-le de 180° (ou vitesse négative) pour qu\'il descende.',
    params: [
      length(2.5, 1),
      width,
      { key: 'heightLow', label: 'Hauteur basse', unit: 'm', min: 0.3, max: 1.5, step: 0.05, default: 0.6 },
      { key: 'heightHigh', label: 'Hauteur haute', unit: 'm', min: 0.4, max: 2.5, step: 0.05, default: 1.3 },
    ],
  },
  {
    kind: 'conveyor-curve',
    label: 'Convoyeur courbe',
    description: 'Virage à bande. L\'angle et le rayon intérieur sont réglables.',
    params: [
      { key: 'angle', label: 'Angle', unit: '°', min: 15, max: 180, step: 15, default: 90 },
      { key: 'innerRadius', label: 'Rayon intérieur', unit: 'm', min: 0.2, max: 3, step: 0.05, default: 0.5 },
      width,
      height,
    ],
  },
  {
    kind: 'conveyor-adjustable',
    label: 'Convoyeur plat réglable',
    description: 'Pieds télescopiques : la hauteur (0,5 à 1,4 m) se règle ensuite dans l\'éditeur 3D.',
    params: [length(2), width],
  },
]

export function getModelType(kind: ModelKind): ModelType {
  return MODEL_TYPES.find((t) => t.kind === kind) ?? MODEL_TYPES[0]!
}

export function defaultParams(kind: ModelKind): Record<string, number> {
  return Object.fromEntries(getModelType(kind).params.map((p) => [p.key, p.default]))
}

// Ramène chaque paramètre dans ses bornes (et complète les manquants)
export function normalizeParams(kind: ModelKind, params: Record<string, number>): Record<string, number> {
  return Object.fromEntries(
    getModelType(kind).params.map((p) => {
      const value = Number(params[p.key])
      return [p.key, Number.isFinite(value) ? Math.min(p.max, Math.max(p.min, value)) : p.default]
    }),
  )
}

export function modelFileName(kind: ModelKind, params: Record<string, number>): string {
  const n = (v: number | undefined) => String(Math.round((v ?? 0) * 100) / 100).replace('.', ',')
  switch (kind) {
    case 'robot-arm':
      return 'bras-robot-6-axes.glb'
    case 'conveyor-straight':
      return `convoyeur-droit-${n(params.length)}m.glb`
    case 'conveyor-incline':
      return `convoyeur-incline-${n(params.length)}m.glb`
    case 'conveyor-curve':
      return `convoyeur-courbe-${n(params.angle)}deg.glb`
    case 'conveyor-adjustable':
      return `convoyeur-plat-reglable-${n(params.length)}m.glb`
  }
}

// Devine le type de modèle à partir du type saisi pour la machine
export function guessModelKind(machineType: string): ModelKind {
  const t = machineType.toLowerCase()
  if (/robot|bras/.test(t)) return 'robot-arm'
  if (/courb|virage|tourn/.test(t)) return 'conveyor-curve'
  if (/inclin|mont|descen/.test(t)) return 'conveyor-incline'
  if (/r[ée]glable|plat/.test(t)) return 'conveyor-adjustable'
  return 'conveyor-straight'
}

// Configurations par défaut proposées pour un modèle généré
// (« Vitesse » et « Hauteur » pilotent l'animation dans le jumeau 3D)
export function suggestedConfigurations(
  kind: ModelKind,
): { name: string; value: number; unit: string; minValue: number; maxValue: number }[] {
  switch (kind) {
    case 'robot-arm':
      return [
        { name: 'Vitesse', value: 100, unit: '%', minValue: 0, maxValue: 200 },
        { name: 'Charge max', value: 10, unit: 'kg', minValue: 0, maxValue: 20 },
      ]
    case 'conveyor-adjustable':
      return [
        { name: 'Vitesse', value: 0.25, unit: 'm/s', minValue: -1, maxValue: 1 },
        { name: 'Hauteur', value: 0.85, unit: 'm', minValue: 0.5, maxValue: 1.4 },
      ]
    default:
      return [{ name: 'Vitesse', value: 0.25, unit: 'm/s', minValue: -1, maxValue: 1 }]
  }
}
