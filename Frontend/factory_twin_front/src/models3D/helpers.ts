import * as THREE from 'three'
import { RoundedBoxGeometry } from 'three/examples/jsm/geometries/RoundedBoxGeometry.js'
import { mergeGeometries } from 'three/examples/jsm/utils/BufferGeometryUtils.js'

// Profil 2D [rayon, hauteur] pour les pièces de révolution
export type Profile = [number, number][]

type Vec3 = [number, number, number]

// Pièce de révolution autour de Y. hard = arêtes vives (une bande par segment de profil)
export function lathe(
  profile: Profile,
  segments = 32,
  options: { phiStart?: number; phiLength?: number; hard?: boolean } = {},
): THREE.BufferGeometry {
  const { phiStart = 0, phiLength = Math.PI * 2, hard = true } = options
  const toVec = (p: [number, number]) => new THREE.Vector2(p[0], p[1])

  if (!hard) {
    return new THREE.LatheGeometry(profile.map(toVec), segments, phiStart, phiLength)
  }

  const parts: THREE.BufferGeometry[] = []
  for (let i = 0; i < profile.length - 1; i++) {
    const a = profile[i]!
    const b = profile[i + 1]!
    if (a[0] === b[0] && a[1] === b[1]) continue
    parts.push(new THREE.LatheGeometry([toVec(a), toVec(b)], segments, phiStart, phiLength))
  }
  const merged = mergeGeometries(parts)
  parts.forEach((g) => g.dispose())
  return merged
}

// Même chose, axe de révolution orienté selon +X
export function latheX(profile: Profile, segments = 32, hard = true) {
  const g = lathe(profile, segments, { hard })
  g.rotateZ(-Math.PI / 2)
  return g
}

// Même chose, axe de révolution orienté selon +Z
export function latheZ(profile: Profile, segments = 32, hard = true) {
  const g = lathe(profile, segments, { hard })
  g.rotateX(Math.PI / 2)
  return g
}

export function cylinderX(radius: number, length: number, segments = 24) {
  const g = new THREE.CylinderGeometry(radius, radius, length, segments)
  g.rotateZ(Math.PI / 2)
  return g
}

export function cylinderZ(radius: number, length: number, segments = 24) {
  const g = new THREE.CylinderGeometry(radius, radius, length, segments)
  g.rotateX(Math.PI / 2)
  return g
}

export function roundedBox(width: number, height: number, depth: number, radius: number) {
  const r = Math.min(radius, width / 2 - 1e-4, height / 2 - 1e-4, depth / 2 - 1e-4)
  return new RoundedBoxGeometry(width, height, depth, 2, Math.max(r, 1e-4))
}

// Extrusion d'une forme 2D (plan XY) selon Z, centrée en Z, avec chanfrein arrondi
export function extrude(shape: THREE.Shape, depth: number, bevel = 0.01, curveSegments = 24) {
  const g = new THREE.ExtrudeGeometry(shape, {
    depth,
    bevelEnabled: bevel > 0,
    bevelThickness: bevel,
    bevelSize: bevel,
    bevelSegments: 2,
    curveSegments,
  })
  g.translate(0, 0, -depth / 2)
  return g
}

export function part(
  geometry: THREE.BufferGeometry,
  material: THREE.Material,
  position: Vec3 = [0, 0, 0],
  name = '',
): THREE.Mesh {
  const mesh = new THREE.Mesh(geometry, material)
  mesh.position.set(...position)
  mesh.castShadow = true
  mesh.receiveShadow = true
  if (name) mesh.name = name
  return mesh
}

export function group(name: string, parent?: THREE.Object3D, position: Vec3 = [0, 0, 0]) {
  const g = new THREE.Group()
  g.name = name
  g.position.set(...position)
  parent?.add(g)
  return g
}

// Moteur électrique à ailettes, axe selon +Z (de z = 0 à z = length)
export function finnedMotor(
  radius: number,
  length: number,
  body: THREE.Material,
  cap: THREE.Material,
): THREE.Group {
  const motor = new THREE.Group()
  motor.name = 'Moteur'

  // Corps à ailettes
  const fins = Math.max(4, Math.round((length * 0.7) / 0.02))
  const start = length * 0.12
  const end = length * 0.82
  const step = (end - start) / fins
  const profile: Profile = [
    [0, 0],
    [radius * 0.8, 0],
    [radius * 0.92, start * 0.6],
    [radius * 0.92, start],
  ]
  for (let i = 0; i < fins; i++) {
    const y = start + i * step
    profile.push([radius, y + step * 0.15], [radius, y + step * 0.6], [radius * 0.92, y + step * 0.75])
  }
  profile.push([radius * 0.92, end], [radius * 0.92, length * 0.86], [0, length * 0.86])
  motor.add(part(latheZ(profile, 28), body))

  // Capot de ventilateur
  motor.add(
    part(
      latheZ(
        [
          [0, 0],
          [radius * 0.95, 0],
          [radius * 0.95, length * 0.12],
          [radius * 0.7, length * 0.16],
          [0, length * 0.16],
        ],
        28,
      ),
      cap,
      [0, 0, length * 0.84],
    ),
  )

  // Boîte à bornes
  motor.add(part(roundedBox(radius * 0.9, radius * 0.5, length * 0.3, 0.008), body, [0, radius * 1.05, length * 0.5]))

  return motor
}
