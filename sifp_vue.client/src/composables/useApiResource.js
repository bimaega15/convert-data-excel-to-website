import { onScopeDispose, ref, shallowRef } from 'vue'

/**
 * Pola pemuatan data yang dipakai seluruh halaman master: panggil sekali saat
 * halaman dibuka, sediakan status loading/error, dan sediakan `reload()`.
 *
 * @param {(signal: AbortSignal) => Promise<any>} loader
 * @param {{ initial?: any, immediate?: boolean }} options
 */
export function useApiResource(loader, { initial = null, immediate = true } = {}) {
  const data = shallowRef(initial)
  const loading = ref(false)
  const error = ref(null)

  let controller = null

  async function reload() {
    // Batalkan permintaan sebelumnya supaya respons lama yang datang terlambat
    // tidak menimpa hasil permintaan terbaru.
    controller?.abort()
    controller = new AbortController()
    const signal = controller.signal

    loading.value = true
    error.value = null

    try {
      const result = await loader(signal)
      if (!signal.aborted) data.value = result
    } catch (err) {
      if (err.name === 'AbortError') return
      error.value = err
    } finally {
      if (!signal.aborted) loading.value = false
    }
  }

  onScopeDispose(() => controller?.abort())

  if (immediate) reload()

  return { data, loading, error, reload }
}

/** Varian untuk endpoint yang mengembalikan daftar baris; `data` selalu array. */
export function useApiRows(loader) {
  const resource = useApiResource(loader, { initial: [] })
  return { rows: resource.data, ...resource }
}
