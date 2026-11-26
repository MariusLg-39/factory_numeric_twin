import * as THREE from 'three'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { RoomEnvironment } from 'three/examples/jsm/environments/RoomEnvironment.js'

// ============================================================
// Vignettes des modèles 3D (rendu hors écran -> image PNG).
// Un seul contexte WebGL partagé ; les rendus passent l'un après l'autre.
// ============================================================

const WIDTH = 320
const HEIGHT = 240

class ThumbnailRenderer {
  private readonly renderer: THREE.WebGLRenderer
  private readonly scene = new THREE.Scene()
  private readonly camera = new THREE.PerspectiveCamera(32, WIDTH / HEIGHT, 0.01, 200)
  private readonly loader = new GLTFLoader()
  private queue: Promise<unknown> = Promise.resolve()

  constructor() {
    this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true })
    this.renderer.setPixelRatio(1)
    this.renderer.setSize(WIDTH, HEIGHT, false)
    this.renderer.setClearColor(0x000000, 0)

    const pmrem = new THREE.PMREMGenerator(this.renderer)
    this.scene.environment = pmrem.fromScene(new RoomEnvironment(), 0.04).texture
    this.scene.environmentIntensity = 0.6
    pmrem.dispose()

    this.scene.add(new THREE.HemisphereLight(0xffffff, 0x94a3b8, 1.2))
    const sun = new THREE.DirectionalLight(0xffffff, 1.8)
    sun.position.set(3, 6, 4)
    this.scene.add(sun)
  }

  // Rend un .glb (ArrayBuffer) ou un objet three.js déjà construit
  render(source: ArrayBuffer | THREE.Object3D): Promise<string> {
    const job = this.queue.then(() => this.renderNow(source))
    this.queue = job.catch(() => undefined)
    return job
  }

  private async renderNow(source: ArrayBuffer | THREE.Object3D): Promise<string> {
    const object = source instanceof ArrayBuffer ? (await this.loader.parseAsync(source, '')).scene : source
    const ownsObject = source instanceof ArrayBuffer

    this.scene.add(object)
    object.updateMatrixWorld(true)

    // Cadrage sur la sphère englobante, vue de 3/4 avant
    const box = new THREE.Box3().setFromObject(object)
    const sphere = box.getBoundingSphere(new THREE.Sphere())
    const radius = Math.max(sphere.radius, 0.05)
    const fov = THREE.MathUtils.degToRad(this.camera.fov)
    const fitHeight = radius / Math.sin(fov / 2)
    const fitWidth = radius / Math.sin(Math.atan(Math.tan(fov / 2) * this.camera.aspect))
    const distance = Math.max(fitHeight, fitWidth) * 0.92
    const direction = new THREE.Vector3(1.1, 0.75, 1.35).normalize()
    this.camera.position.copy(sphere.center).addScaledVector(direction, distance)
    this.camera.near = distance / 100
    this.camera.far = distance * 10
    this.camera.updateProjectionMatrix()
    this.camera.lookAt(sphere.center)

    this.renderer.render(this.scene, this.camera)
    const url = this.renderer.domElement.toDataURL('image/png')

    this.scene.remove(object)
    if (ownsObject) disposeObject(object)
    return url
  }
}

let shared: ThumbnailRenderer | null = null

function renderer() {
  shared ??= new ThumbnailRenderer()
  return shared
}

// Cache mémoire : la vignette d'une machine ne change que si son modèle change
const cache = new Map<string, Promise<string>>()

export function renderThumbnail(source: ArrayBuffer | THREE.Object3D): Promise<string> {
  return renderer().render(source)
}

export function cachedThumbnail(key: string, load: () => Promise<ArrayBuffer>): Promise<string> {
  let entry = cache.get(key)
  if (!entry) {
    entry = load().then((buffer) => renderer().render(buffer))
    entry.catch(() => cache.delete(key))
    cache.set(key, entry)
  }
  return entry
}

export function disposeObject(object: THREE.Object3D) {
  object.traverse((child) => {
    const mesh = child as THREE.Mesh
    if (!mesh.isMesh) return
    mesh.geometry.dispose()
    const materials = Array.isArray(mesh.material) ? mesh.material : [mesh.material]
    for (const material of materials) {
      for (const value of Object.values(material)) {
        if (value instanceof THREE.Texture) value.dispose()
      }
      material.dispose()
    }
  })
}
