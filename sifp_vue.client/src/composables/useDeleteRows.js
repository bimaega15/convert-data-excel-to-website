import { ref } from 'vue'

/**
 * Logika hapus baris untuk tabel master: jalankan fungsi hapus, muat ulang data,
 * dan sediakan status loading/error untuk tombol & banner di DataTable.
 *
 * @param {(keys: (number|string)[]) => Promise<any>} deleteFn  memanggil endpoint hapus
 * @param {() => Promise<any>} reload  memuat ulang baris setelah hapus berhasil
 */
export function useDeleteRows(deleteFn, reload) {
  const deleting = ref(false)
  const deleteError = ref(null)

  async function onDelete(keys) {
    if (!keys?.length || deleting.value) return
    deleting.value = true
    deleteError.value = null
    try {
      await deleteFn(keys)
      await reload()
    } catch (err) {
      deleteError.value = err
    } finally {
      deleting.value = false
    }
  }

  return { deleting, deleteError, onDelete }
}
