import * as THREE from 'three'

export const BELT_MATERIAL_NAME = 'Bande'
// La texture de la bande se répète tous les 25 cm
export const BELT_UV_PER_METER = 4

function standard(name: string, color: number, metalness: number, roughness: number) {
  const material = new THREE.MeshStandardMaterial({ color, metalness, roughness })
  material.name = name
  return material
}

// Nouvelle palette de matériaux pour chaque modèle (pas de partage entre fichiers)
export function createMaterials() {
  return {
    orange: standard('Peinture orange', 0xf08a24, 0.25, 0.42),
    darkGrey: standard('Fonte grise', 0x434a54, 0.4, 0.5),
    black: standard('Caoutchouc', 0x1d2025, 0.0, 0.85),
    steel: standard('Acier', 0xc9cfd6, 0.6, 0.3),
    aluminium: standard('Aluminium', 0xdfe3e8, 0.45, 0.38),
    frame: standard('Châssis', 0xa3abb5, 0.35, 0.45),
    motor: standard('Moteur', 0x2f5687, 0.45, 0.45),
    yellow: standard('Sécurité', 0xf5c518, 0.1, 0.55),
  }
}

export type Materials = ReturnType<typeof createMaterials>

// Bande transporteuse : caoutchouc noir avec chevrons.
// L'axe U de la texture suit le sens de défilement : décaler texture.offset.x l'anime.
export function createBeltMaterial(): THREE.MeshStandardMaterial {
  const size = 256
  const canvas = document.createElement('canvas')
  canvas.width = size
  canvas.height = size
  const ctx = canvas.getContext('2d')!

  ctx.fillStyle = '#25282e'
  ctx.fillRect(0, 0, size, size)

  // Grain du caoutchouc
  for (let i = 0; i < 1800; i++) {
    const shade = 30 + Math.floor(Math.random() * 18)
    ctx.fillStyle = `rgb(${shade},${shade + 2},${shade + 6})`
    ctx.fillRect(Math.random() * size, Math.random() * size, 2, 2)
  }

  // Deux chevrons par motif (un tous les 12,5 cm)
  ctx.strokeStyle = '#4a515c'
  ctx.lineWidth = 16
  ctx.lineCap = 'round'
  ctx.lineJoin = 'round'
  for (const x of [40, 168]) {
    ctx.beginPath()
    ctx.moveTo(x, 22)
    ctx.lineTo(x + 46, size / 2)
    ctx.lineTo(x, size - 22)
    ctx.stroke()
  }

  // Bords de bande légèrement plus clairs
  ctx.fillStyle = '#3a3f47'
  ctx.fillRect(0, 0, size, 6)
  ctx.fillRect(0, size - 6, size, 6)

  const texture = new THREE.CanvasTexture(canvas)
  texture.name = 'Bande'
  texture.colorSpace = THREE.SRGBColorSpace
  texture.wrapS = THREE.RepeatWrapping
  texture.wrapT = THREE.ClampToEdgeWrapping
  texture.anisotropy = 4

  const material = new THREE.MeshStandardMaterial({
    map: texture,
    metalness: 0,
    roughness: 0.88,
  })
  material.name = BELT_MATERIAL_NAME
  return material
}
