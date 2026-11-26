import * as THREE from 'three'
import { BELT_MATERIAL_NAME, BELT_UV_PER_METER, createBeltMaterial, createMaterials, type Materials } from './materials'
import { cylinderZ, extrude, finnedMotor, group, lathe, part, roundedBox, type Profile } from './helpers'
import { RIG_KEY, type BuiltModel, type RigMeta } from './types'

// ============================================================
// Convoyeurs à bande. Unités : mètres. Origine au sol.
// Le dessus de la bande est le repère « hauteur » du convoyeur.
//
// Nœuds utiles pour l'animation :
//   Corps       : tout ce qui est au-dessus des pieds (bande, rives, moteur)
//   Bande       : matériau de la bande, texture à faire défiler (offset.x)
//   Entree / Sortie : points de dépose / récupération des pièces
// ============================================================

const RAIL_W = 0.035 // largeur d'une rive
const RAIL_GAP = 0.004 // jeu entre bande et rive
const RAIL_TOP = 0.035 // la rive dépasse de la bande
const RAIL_BOTTOM = -0.13
const DRUM_R = 0.045 // rayon des tambours
const LEG = 0.05 // section des pieds
const FOOT_H = 0.05 // hauteur des vérins de pied
const BRACE_Y = 0.2 // hauteur des traverses basses
const DEFAULT_SPEED = 0.25 // m/s

export interface StraightParams {
  length: number
  width: number
  height: number
}

export interface InclineParams {
  length: number
  width: number
  heightLow: number
  heightHigh: number
}

export interface CurveParams {
  angle: number // degrés
  innerRadius: number
  width: number
  height: number
}

export interface AdjustableParams {
  length: number
  width: number
}

export const ADJUSTABLE_HEIGHT = { min: 0.5, max: 1.4, default: 0.85 }

function conveyorMeta(variant: RigMeta['variant']): RigMeta {
  return {
    kind: 'conveyor',
    variant,
    version: 1,
    beltMaterial: BELT_MATERIAL_NAME,
    uvPerMeter: BELT_UV_PER_METER,
    speed: DEFAULT_SPEED,
  }
}

function marker(name: string, parent: THREE.Object3D, position: [number, number, number]) {
  const o = new THREE.Object3D()
  o.name = name
  o.position.set(...position)
  parent.add(o)
  return o
}

// ============================================================
// SURFACE DE BANDE (UV en mètres le long du trajet)
// ============================================================

type PathPoint = { x: number; z: number; nx: number; nz: number }

function beltSurface(length: number, width: number, segments: number, at: (s: number) => PathPoint) {
  const positions: number[] = []
  const normals: number[] = []
  const uvs: number[] = []
  const indices: number[] = []
  const w = width / 2

  for (let i = 0; i <= segments; i++) {
    const s = (length * i) / segments
    const p = at(s)
    positions.push(p.x - p.nx * w, 0, p.z - p.nz * w, p.x + p.nx * w, 0, p.z + p.nz * w)
    normals.push(0, 1, 0, 0, 1, 0)
    uvs.push(s * BELT_UV_PER_METER, 0, s * BELT_UV_PER_METER, 1)

    if (i < segments) {
      const a = i * 2
      indices.push(a, a + 1, a + 2, a + 1, a + 3, a + 2)
    }
  }

  const g = new THREE.BufferGeometry()
  g.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3))
  g.setAttribute('normal', new THREE.Float32BufferAttribute(normals, 3))
  g.setAttribute('uv', new THREE.Float32BufferAttribute(uvs, 2))
  g.setIndex(indices)
  return g
}

// Profil de rive [décalage latéral, y] : face lisse côté bande, rainure en T à l'extérieur,
// lèvre qui recouvre le bord de la bande. dir = +1 : la rive s'étend vers l'extérieur.
function railProfile(edge: number, dir: number): Profile {
  const outer = edge + dir * RAIL_W
  const notch = outer - dir * 0.008
  const lip = edge - dir * 0.015
  return [
    [edge, RAIL_BOTTOM],
    [outer, RAIL_BOTTOM],
    [outer, -0.085],
    [notch, -0.075],
    [notch, -0.035],
    [outer, -0.025],
    [outer, RAIL_TOP],
    [lip, RAIL_TOP],
    [lip, RAIL_TOP - 0.012],
    [edge, RAIL_TOP - 0.012],
    [edge, RAIL_BOTTOM],
  ]
}

// ============================================================
// CORPS DROIT (bande + rives + tambours + motorisation), origine = dessus de bande au centre
// ============================================================

function straightBody(m: Materials, belt: THREE.Material, length: number, width: number): THREE.Group {
  const body = new THREE.Group()
  body.name = 'Corps'
  const halfL = length / 2

  // Dessus de bande texturé
  body.add(
    part(
      beltSurface(length, width - 0.006, Math.max(1, Math.round(length * 4)), (s) => ({
        x: s - halfL,
        z: 0,
        nx: 0,
        nz: 1,
      })),
      belt,
      [0, 0.0006, 0],
      'BandeDessus',
    ),
  )

  // Bande enroulée autour des deux tambours (profil en stade)
  const cy = -DRUM_R - 0.001
  const loop = new THREE.Shape()
  loop.moveTo(-halfL, 0)
  loop.lineTo(halfL, 0)
  loop.absarc(halfL, cy, DRUM_R + 0.001, Math.PI / 2, -Math.PI / 2, true)
  loop.lineTo(-halfL, cy - DRUM_R - 0.001)
  loop.absarc(-halfL, cy, DRUM_R + 0.001, -Math.PI / 2, Math.PI / 2, true)
  body.add(part(extrude(loop, width - 0.006, 0, 24), m.black))

  // Axes des tambours (dépassent des rives)
  const axleLength = width + 2 * (RAIL_GAP + RAIL_W) + 0.024
  for (const x of [-halfL, halfL]) {
    body.add(part(cylinderZ(0.018, axleLength, 16), m.steel, [x, cy, 0]))
  }

  // Rives en profilé aluminium
  const railLength = length + 2 * DRUM_R + 0.05
  for (const side of [-1, 1]) {
    const shape = new THREE.Shape(
      railProfile(width / 2 + RAIL_GAP, 1).map(([offset, y]) => new THREE.Vector2(-side * offset, y)),
    )
    const g = extrude(shape, railLength, 0.002, 2)
    g.rotateY(Math.PI / 2)
    body.add(part(g, m.aluminium))

    // Embouts plastiques
    for (const x of [-railLength / 2 - 0.004, railLength / 2 + 0.004]) {
      body.add(
        part(
          roundedBox(0.008, RAIL_TOP - RAIL_BOTTOM, RAIL_W, 0.002),
          m.black,
          [x, (RAIL_TOP + RAIL_BOTTOM) / 2, side * (width / 2 + RAIL_GAP + RAIL_W / 2)],
        ),
      )
    }
  }

  // Motoréducteur côté sortie
  addDrive(body, m, halfL - 0.02, cy, width / 2 + RAIL_GAP + RAIL_W, 0)

  marker('Entree', body, [-halfL + 0.1, 0.002, 0])
  marker('Sortie', body, [halfL - 0.1, 0.002, 0])

  return body
}

// Réducteur + moteur fixés sur la rive, orientés vers l'extérieur.
function addDrive(parent: THREE.Object3D, m: Materials, x: number, y: number, outerEdge: number, angle: number) {
  const drive = new THREE.Group()
  drive.name = 'Motorisation'
  drive.add(part(roundedBox(0.15, 0.15, 0.12, 0.018), m.motor, [0, 0, 0.06]))
  drive.add(part(cylinderZ(0.05, 0.02, 24), m.darkGrey, [0, 0, 0.13]))
  const motor = finnedMotor(0.065, 0.22, m.motor, m.darkGrey)
  motor.position.set(0, 0, 0.14)
  drive.add(motor)

  // drive est construit pour un côté +Z, puis tourné autour de Y
  drive.position.set(x, y, outerEdge)
  if (angle) {
    const holder = new THREE.Group()
    holder.rotation.y = angle
    holder.add(drive)
    parent.add(holder)
  } else {
    parent.add(drive)
  }
}

// ============================================================
// PIEDS
// ============================================================

function foot(m: Materials) {
  return part(
    lathe(
      [
        [0, 0],
        [0.035, 0],
        [0.035, 0.012],
        [0.012, 0.02],
        [0.012, FOOT_H],
        [0, FOOT_H],
      ],
      20,
    ),
    m.black,
  )
}

// Pied vertical de bas (pied au sol) jusqu'à top
function leg(parent: THREE.Object3D, m: Materials, x: number, z: number, top: number) {
  const f = foot(m)
  f.position.set(x, 0, z)
  parent.add(f)
  const height = Math.max(0.05, top - FOOT_H)
  parent.add(part(roundedBox(LEG, height, LEG, 0.005), m.frame, [x, FOOT_H + height / 2, z]))
}

function legPositions(length: number): number[] {
  const usable = length - 0.3
  const count = length > 2.5 ? Math.ceil(usable / 1.5) + 1 : 2
  return Array.from({ length: count }, (_, i) => -usable / 2 + (usable * i) / (count - 1))
}

function braces(parent: THREE.Object3D, m: Materials, xs: number[], legZ: number) {
  for (const x of xs) {
    parent.add(part(roundedBox(0.04, 0.04, legZ * 2, 0.004), m.frame, [x, BRACE_Y, 0]))
  }
  const span = xs[xs.length - 1]! - xs[0]!
  if (span > 0) {
    for (const side of [-1, 1]) {
      parent.add(part(roundedBox(span, 0.04, 0.04, 0.004), m.frame, [(xs[0]! + xs[xs.length - 1]!) / 2, BRACE_Y + 0.05, side * legZ]))
    }
  }
}

// ============================================================
// CONVOYEUR DROIT
// ============================================================

export function buildStraightConveyor(p: StraightParams): BuiltModel {
  const m = createMaterials()
  const root = group('ConvoyeurDroit')
  root.userData[RIG_KEY] = conveyorMeta('straight')

  const body = straightBody(m, createBeltMaterial(), p.length, p.width)
  body.position.y = p.height
  root.add(body)

  const legs = group('Pieds', root)
  const xs = legPositions(p.length)
  const legZ = p.width / 2 + RAIL_GAP + RAIL_W / 2
  for (const x of xs) for (const side of [-1, 1]) leg(legs, m, x, side * legZ, p.height + RAIL_BOTTOM)
  braces(legs, m, xs, legZ)

  return { root, animations: [] }
}

// ============================================================
// CONVOYEUR INCLINÉ (monte vers +X ; tourner de 180° pour qu'il descende,
// ou passer une vitesse négative)
// ============================================================

export function buildInclineConveyor(p: InclineParams): BuiltModel {
  const m = createMaterials()
  const root = group('ConvoyeurIncline')
  root.userData[RIG_KEY] = conveyorMeta('incline')

  const rise = p.heightHigh - p.heightLow
  const slope = Math.asin(THREE.MathUtils.clamp(rise / p.length, -0.9, 0.9))
  const centerY = (p.heightLow + p.heightHigh) / 2

  const body = straightBody(m, createBeltMaterial(), p.length, p.width)
  body.rotation.z = slope
  body.position.y = centerY
  root.add(body)

  // Pieds verticaux de hauteurs différentes sous chaque point d'appui
  const legs = group('Pieds', root)
  const legZ = p.width / 2 + RAIL_GAP + RAIL_W / 2
  const along = legPositions(p.length)
  const xs: number[] = []
  for (const s of along) {
    const x = s * Math.cos(slope)
    const top = centerY + s * Math.sin(slope) + RAIL_BOTTOM * Math.cos(slope)
    xs.push(x)
    for (const side of [-1, 1]) leg(legs, m, x, side * legZ, top)
    // Chape d'articulation en haut du pied
    for (const side of [-1, 1]) {
      legs.add(part(cylinderZ(0.016, 0.07, 12), m.steel, [x, top - 0.01, side * legZ]))
    }
  }
  braces(legs, m, xs, legZ)

  return { root, animations: [] }
}

// ============================================================
// CONVOYEUR COURBE (virage à gauche dans le sens de la bande)
// ============================================================

export function buildCurveConveyor(p: CurveParams): BuiltModel {
  const m = createMaterials()
  const belt = createBeltMaterial()
  const root = group('ConvoyeurCourbe')
  root.userData[RIG_KEY] = conveyorMeta('curve')

  const angle = THREE.MathUtils.degToRad(p.angle)
  const ri = p.innerRadius
  const ro = ri + p.width
  const rm = (ri + ro) / 2
  const arc = rm * angle
  const segments = Math.max(12, Math.round(p.angle / 3))

  const body = group('Corps', root, [0, p.height, 0])

  // Dessus de bande (u = longueur parcourue au rayon moyen)
  body.add(
    part(
      beltSurface(arc, p.width - 0.006, segments, (s) => {
        const t = s / rm
        return { x: rm * Math.sin(t), z: rm * Math.cos(t), nx: Math.sin(t), nz: Math.cos(t) }
      }),
      belt,
      [0, 0.0006, 0],
      'BandeDessus',
    ),
  )

  // Épaisseur de bande
  body.add(
    part(
      lathe(
        [
          [ri + 0.003, -0.001],
          [ro - 0.003, -0.001],
          [ro - 0.003, -0.05],
          [ri + 0.003, -0.05],
          [ri + 0.003, -0.001],
        ],
        segments,
        { phiLength: angle },
      ),
      m.black,
    ),
  )

  // Rouleaux d'extrémité + axes
  const cy = -DRUM_R - 0.001
  const axleLength = p.width + 2 * (RAIL_GAP + RAIL_W) + 0.024
  for (const a of [0, angle]) {
    const holder = group('', body)
    holder.rotation.y = a
    holder.add(part(cylinderZ(DRUM_R + 0.001, p.width - 0.006, 24), m.black, [0, cy, rm]))
    holder.add(part(cylinderZ(0.018, axleLength, 16), m.steel, [0, cy, rm]))
  }

  // Rives intérieure et extérieure + embouts
  for (const [edge, dir] of [
    [ri - RAIL_GAP, -1],
    [ro + RAIL_GAP, 1],
  ] as const) {
    body.add(part(lathe(railProfile(edge, dir), segments, { phiLength: angle }), m.aluminium))
    const center = edge + (dir * RAIL_W) / 2
    for (const a of [0, angle]) {
      const cap = part(roundedBox(0.008, RAIL_TOP - RAIL_BOTTOM, RAIL_W, 0.002), m.black, [
        center * Math.sin(a),
        (RAIL_TOP + RAIL_BOTTOM) / 2,
        center * Math.cos(a),
      ])
      cap.rotation.y = a
      body.add(cap)
    }
  }

  // Motorisation sur la rive extérieure, côté sortie
  addDrive(body, m, -0.06, cy, ro + RAIL_GAP + RAIL_W, angle)

  // Repères d'entrée / sortie
  const t1 = 0.1 / rm
  const t2 = angle - 0.1 / rm
  marker('Entree', body, [rm * Math.sin(t1), 0.002, rm * Math.cos(t1)])
  marker('Sortie', body, [rm * Math.sin(t2), 0.002, rm * Math.cos(t2)])

  // Pieds sur deux rayons, reliés par des traverses radiales
  const legs = group('Pieds', root)
  const rIn = ri - RAIL_GAP - RAIL_W / 2
  const rOut = ro + RAIL_GAP + RAIL_W / 2
  const legAngles = p.angle > 120 ? [0.12, 0.5, 0.88] : [0.18, 0.82]
  for (const f of legAngles) {
    const a = angle * f
    for (const r of [rIn, rOut]) leg(legs, m, r * Math.sin(a), r * Math.cos(a), p.height + RAIL_BOTTOM)
    const brace = part(roundedBox(0.04, 0.04, rOut - rIn, 0.004), m.frame, [
      ((rIn + rOut) / 2) * Math.sin(a),
      BRACE_Y,
      ((rIn + rOut) / 2) * Math.cos(a),
    ])
    brace.rotation.y = a
    legs.add(brace)
  }

  // Origine = centre de l'emprise au sol (plus pratique à placer dans l'éditeur)
  centerFootprint(root)

  return { root, animations: [] }
}

// ============================================================
// CONVOYEUR PLAT À HAUTEUR RÉGLABLE (pieds télescopiques)
//
// Hauteur réglée par machineRigs.setHeight :
//   Corps.position.y = hauteur
//   JambeInterieure*.scale.y = hauteur + topOffset - legBase
// ============================================================

const OUTER_LEG_TOP = 0.37 // haut des tubes extérieurs fixes
const INNER_LEG_BASE = 0.3 // bas des tubes intérieurs coulissants

export function buildAdjustableConveyor(p: AdjustableParams): BuiltModel {
  const m = createMaterials()
  const root = group('ConvoyeurReglable')
  const meta = conveyorMeta('adjustable')
  meta.height = {
    ...ADJUSTABLE_HEIGHT,
    bodyNode: 'Corps',
    legPrefix: 'JambeInterieure',
    legBase: INNER_LEG_BASE,
    topOffset: RAIL_BOTTOM,
  }
  root.userData[RIG_KEY] = meta

  const height = ADJUSTABLE_HEIGHT.default
  const body = straightBody(m, createBeltMaterial(), p.length, p.width)
  body.position.y = height
  root.add(body)

  const legs = group('Pieds', root)
  const xs = legPositions(p.length)
  const legZ = p.width / 2 + RAIL_GAP + RAIL_W / 2

  // Tube intérieur : hauteur 1 m, origine en bas -> étiré par scale.y
  const innerGeometry = roundedBox(LEG * 0.78, 1, LEG * 0.78, 0.004)
  innerGeometry.translate(0, 0.5, 0)

  let index = 1
  for (const x of xs) {
    for (const side of [-1, 1]) {
      const z = side * legZ
      const f = foot(m)
      f.position.set(x, 0, z)
      legs.add(f)

      // Tube extérieur fixe
      const outerHeight = OUTER_LEG_TOP - FOOT_H
      legs.add(part(roundedBox(LEG, outerHeight, LEG, 0.005), m.frame, [x, FOOT_H + outerHeight / 2, z]))
      // Collier de serrage + manette
      legs.add(part(roundedBox(LEG + 0.012, 0.03, LEG + 0.012, 0.005), m.darkGrey, [x, OUTER_LEG_TOP - 0.02, z]))
      const knob = part(cylinderZ(0.006, 0.05, 10), m.steel, [x, OUTER_LEG_TOP - 0.02, z + side * 0.05])
      legs.add(knob)
      legs.add(part(new THREE.SphereGeometry(0.014, 12, 8), m.yellow, [x, OUTER_LEG_TOP - 0.02, z + side * 0.078]))

      // Tube intérieur coulissant
      const inner = part(innerGeometry, m.steel, [x, INNER_LEG_BASE, z], `JambeInterieure${index++}`)
      inner.scale.y = height + RAIL_BOTTOM - INNER_LEG_BASE
      legs.add(inner)
    }
  }
  braces(legs, m, xs, legZ)

  return { root, animations: [] }
}

function centerFootprint(root: THREE.Object3D) {
  root.updateMatrixWorld(true)
  const box = new THREE.Box3().setFromObject(root)
  const center = box.getCenter(new THREE.Vector3())
  for (const child of root.children) {
    child.position.x -= center.x
    child.position.z -= center.z
  }
}
