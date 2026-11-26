import * as THREE from 'three'
import { createMaterials } from './materials'
import { cylinderX, cylinderZ, extrude, finnedMotor, group, lathe, latheX, part, roundedBox } from './helpers'
import { RIG_KEY, type BuiltModel, type RigMeta } from './types'

// ============================================================
// Bras robot industriel 6 axes (portée ~1,4 m)
//
// Hiérarchie (chaque axe est un nœud pivot nommé, animable avec three.js) :
//   BrasRobot
//   └─ Base
//      └─ Axe1 (rotation Y : tourelle)
//         └─ Axe2 (rotation Z : épaule)
//            └─ Axe3 (rotation Z : coude)
//               └─ Axe4 (rotation X : avant-bras)
//                  └─ Axe5 (rotation Z : poignet)
//                     └─ Axe6 (rotation X : bride)
//                        └─ Pince ─ DoigtGauche / DoigtDroit (translation Z)
//                                 └─ PointOutil
// Position de repos : bras vertical, avant-bras horizontal vers +X.
// ============================================================

export const ROBOT_CLIP = 'Travail'

const SHOULDER: [number, number, number] = [0.08, 0.45, 0]
const UPPER_ARM = 0.7
const FINGER_OPEN = 0.045
const FINGER_CLOSED = 0.016

export function buildRobotArm(): BuiltModel {
  const m = createMaterials()

  const root = group('BrasRobot')
  const meta: RigMeta = { kind: 'robot-arm', version: 1, clip: ROBOT_CLIP }
  root.userData[RIG_KEY] = meta

  // ---------------- Base fixe ----------------
  const base = group('Base', root)
  base.add(
    part(
      lathe(
        [
          [0, 0],
          [0.32, 0],
          [0.32, 0.035],
          [0.29, 0.05],
          [0.255, 0.06],
          [0.24, 0.16],
          [0, 0.16],
        ],
        48,
      ),
      m.darkGrey,
    ),
  )
  // Bande de sécurité jaune
  base.add(part(lathe([[0.322, 0.012], [0.322, 0.028]], 48), m.yellow))
  // Boulons d'ancrage
  for (let i = 0; i < 8; i++) {
    const a = (i / 8) * Math.PI * 2 + Math.PI / 8
    base.add(part(new THREE.CylinderGeometry(0.014, 0.014, 0.02, 6), m.steel, [Math.cos(a) * 0.295, 0.045, Math.sin(a) * 0.295]))
  }

  // ---------------- Axe 1 : tourelle ----------------
  const axe1 = group('Axe1', base, [0, 0.16, 0])
  axe1.add(
    part(
      lathe(
        [
          [0, 0],
          [0.25, 0],
          [0.25, 0.025],
          [0.23, 0.05],
          [0.21, 0.2],
          [0.17, 0.24],
          [0, 0.24],
        ],
        48,
      ),
      m.orange,
    ),
  )
  // Moteur de l'axe 1 (à l'arrière, vertical)
  const motor1 = finnedMotor(0.065, 0.2, m.darkGrey, m.black)
  motor1.rotation.x = -Math.PI / 2
  motor1.position.set(-0.19, 0.16, 0)
  axe1.add(motor1)

  // Joues de l'épaule (fourche)
  const cheek = new THREE.Shape()
  cheek.moveTo(-0.15, 0.2)
  cheek.lineTo(0.22, 0.2)
  cheek.lineTo(0.23, SHOULDER[1])
  cheek.absarc(SHOULDER[0], SHOULDER[1], 0.15, 0, Math.PI, false)
  cheek.lineTo(-0.15, 0.33)
  cheek.closePath()
  for (const side of [-1, 1]) {
    axe1.add(part(extrude(cheek, 0.06, 0.012), m.orange, [0, 0, side * 0.14]))
    // Capots d'articulation
    axe1.add(part(cylinderZ(0.12, 0.016, 40), m.darkGrey, [SHOULDER[0], SHOULDER[1], side * 0.19]))
  }
  // Moteur de l'axe 2 (sur le côté)
  const motor2 = finnedMotor(0.075, 0.15, m.darkGrey, m.black)
  motor2.rotation.y = Math.PI
  motor2.position.set(SHOULDER[0], SHOULDER[1], -0.2)
  axe1.add(motor2)

  // ---------------- Axe 2 : épaule + bras ----------------
  const axe2 = group('Axe2', axe1, SHOULDER)
  axe2.add(part(cylinderZ(0.1, 0.2, 40), m.darkGrey))

  const upperArm = new THREE.Shape()
  const r1 = 0.105
  const r2 = 0.085
  upperArm.moveTo(-r1, 0)
  upperArm.lineTo(-r2, UPPER_ARM)
  upperArm.absarc(0, UPPER_ARM, r2, Math.PI, 0, true)
  upperArm.lineTo(r1, 0)
  upperArm.absarc(0, 0, r1, 0, Math.PI, true)
  axe2.add(part(extrude(upperArm, 0.13, 0.018, 32), m.orange))

  // Faisceau de câbles le long du bras
  const cable = new THREE.CatmullRomCurve3([
    new THREE.Vector3(-0.1, -0.02, 0.1),
    new THREE.Vector3(-0.135, 0.25, 0.105),
    new THREE.Vector3(-0.12, 0.5, 0.1),
    new THREE.Vector3(-0.07, UPPER_ARM + 0.02, 0.1),
  ])
  axe2.add(part(new THREE.TubeGeometry(cable, 24, 0.013, 8), m.black))

  // ---------------- Axe 3 : coude + carter d'avant-bras ----------------
  const axe3 = group('Axe3', axe2, [0, UPPER_ARM, 0])
  axe3.add(part(cylinderZ(0.088, 0.2, 40), m.darkGrey))
  axe3.add(part(roundedBox(0.34, 0.17, 0.19, 0.035), m.orange, [-0.04, 0.01, 0]))
  // Moteur de l'axe 3 (côté) et de l'axe 4 (arrière)
  const motor3 = finnedMotor(0.06, 0.1, m.darkGrey, m.black)
  motor3.position.set(0, 0, 0.1)
  axe3.add(motor3)
  const motor4 = finnedMotor(0.055, 0.12, m.darkGrey, m.black)
  motor4.rotation.y = -Math.PI / 2
  motor4.position.set(-0.2, 0.01, 0)
  axe3.add(motor4)

  // ---------------- Axe 4 : rotation de l'avant-bras ----------------
  const axe4 = group('Axe4', axe3, [0.13, 0, 0])
  axe4.add(
    part(
      latheX(
        [
          [0, 0],
          [0.08, 0],
          [0.08, 0.04],
          [0.068, 0.1],
          [0.058, 0.44],
          [0.062, 0.46],
          [0, 0.46],
        ],
        40,
        false,
      ),
      m.orange,
    ),
  )
  // Fourche du poignet
  for (const side of [-1, 1]) {
    axe4.add(part(roundedBox(0.13, 0.1, 0.022, 0.01), m.orange, [0.5, 0, side * 0.05]))
  }

  // ---------------- Axe 5 : poignet ----------------
  const axe5 = group('Axe5', axe4, [0.52, 0, 0])
  axe5.add(part(cylinderZ(0.045, 0.078, 32), m.darkGrey))
  axe5.add(
    part(
      latheX(
        [
          [0, 0],
          [0.042, 0],
          [0.042, 0.06],
          [0.036, 0.075],
          [0, 0.075],
        ],
        32,
      ),
      m.darkGrey,
    ),
  )

  // ---------------- Axe 6 : bride + pince ----------------
  const axe6 = group('Axe6', axe5, [0.075, 0, 0])
  axe6.add(part(cylinderX(0.045, 0.016, 32), m.steel, [0.008, 0, 0]))
  for (let i = 0; i < 6; i++) {
    const a = (i / 6) * Math.PI * 2
    axe6.add(part(cylinderX(0.006, 0.006, 6), m.darkGrey, [0.018, Math.cos(a) * 0.034, Math.sin(a) * 0.034]))
  }

  const gripper = group('Pince', axe6, [0.016, 0, 0])
  gripper.add(part(roundedBox(0.06, 0.07, 0.15, 0.012), m.darkGrey, [0.03, 0, 0]))
  gripper.add(part(roundedBox(0.012, 0.05, 0.13, 0.004), m.steel, [0.064, 0, 0])) // rail de guidage

  for (const [name, side] of [
    ['DoigtGauche', 1],
    ['DoigtDroit', -1],
  ] as const) {
    const finger = group(name, gripper, [0.06, 0, side * FINGER_OPEN])
    finger.add(part(roundedBox(0.09, 0.034, 0.016, 0.005), m.steel, [0.045, 0, 0]))
    finger.add(part(roundedBox(0.04, 0.03, 0.006, 0.002), m.black, [0.07, 0, -side * 0.011])) // patin
  }

  const tool = new THREE.Object3D()
  tool.name = 'PointOutil'
  tool.position.set(0.16, 0, 0)
  gripper.add(tool)

  return { root, animations: [buildPickAndPlaceClip()] }
}

// ============================================================
// Clip « Travail » (8 s, en boucle) : prise à droite, rotation de la pièce, dépose à gauche.
// Angles calculés pour que la pince prenne à ~0,9 m du robot et ~0,86 m du sol
// (hauteur d'un convoyeur standard).
// ============================================================

type Pose = [number, number, number, number, number, number, number] // A1..A6 en degrés, pince 0 = ouverte / 1 = fermée

const ABOVE = [-14, 11, 0, -87] as const // A2, A3, A4, A5 : 15 cm au-dessus de la pièce
const PICK = [-16, 0, 0, -74] as const // A2, A3, A4, A5 : pince au contact

const KEYFRAMES: [number, Pose][] = [
  [0.0, [0, 0, 0, 0, 0, 0, 0]],
  [1.2, [55, ...ABOVE, 0, 0]],
  [1.9, [55, ...PICK, 0, 0]],
  [2.4, [55, ...PICK, 0, 1]],
  [3.0, [55, ...ABOVE, 0, 1]],
  [4.6, [-55, ...ABOVE, 90, 1]],
  [5.2, [-55, ...PICK, 90, 1]],
  [5.7, [-55, ...PICK, 90, 0]],
  [6.3, [-55, ...ABOVE, 90, 0]],
  [8.0, [0, 0, 0, 0, 0, 0, 0]],
]

const AXES: [string, THREE.Vector3][] = [
  ['Axe1', new THREE.Vector3(0, 1, 0)],
  ['Axe2', new THREE.Vector3(0, 0, 1)],
  ['Axe3', new THREE.Vector3(0, 0, 1)],
  ['Axe4', new THREE.Vector3(1, 0, 0)],
  ['Axe5', new THREE.Vector3(0, 0, 1)],
  ['Axe6', new THREE.Vector3(1, 0, 0)],
]

function buildPickAndPlaceClip(): THREE.AnimationClip {
  const times = KEYFRAMES.map(([t]) => t)
  const tracks: THREE.KeyframeTrack[] = []
  const q = new THREE.Quaternion()

  AXES.forEach(([name, axis], index) => {
    const values: number[] = []
    for (const [, pose] of KEYFRAMES) {
      q.setFromAxisAngle(axis, THREE.MathUtils.degToRad(pose[index]!))
      values.push(q.x, q.y, q.z, q.w)
    }
    tracks.push(new THREE.QuaternionKeyframeTrack(`${name}.quaternion`, times, values))
  })

  for (const [name, side] of [
    ['DoigtGauche', 1],
    ['DoigtDroit', -1],
  ] as const) {
    const values: number[] = []
    for (const [, pose] of KEYFRAMES) {
      const z = pose[6] ? FINGER_CLOSED : FINGER_OPEN
      values.push(0.06, 0, side * z)
    }
    tracks.push(new THREE.VectorKeyframeTrack(`${name}.position`, times, values))
  }

  return new THREE.AnimationClip(ROBOT_CLIP, 8, tracks)
}
