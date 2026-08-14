// Sesi login klien Vue: token bearer + data user, disimpan di localStorage supaya
// tetap login setelah reload. Mengikuti pola singleton reaktif yang sama dengan
// src/data/dashboard.js (bukan Pinia — proyek ini tidak memakainya).

import { reactive } from 'vue'

const STORAGE_KEY = 'sifp.auth'

export const authState = reactive({
  token: null,
  expiresAtUtc: null,
  user: null,
})

function restore() {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return

  try {
    const saved = JSON.parse(raw)
    authState.token = saved.token ?? null
    authState.expiresAtUtc = saved.expiresAtUtc ?? null
    authState.user = saved.user ?? null
  } catch {
    localStorage.removeItem(STORAGE_KEY)
  }
}

/** Token ada dan belum lewat masa berlakunya. Murni pengecekan lokal, tanpa panggilan API. */
export function isTokenValid() {
  if (!authState.token || !authState.expiresAtUtc) return false
  return new Date(authState.expiresAtUtc).getTime() > Date.now()
}

/** Dipanggil setelah login (manual atau Windows) berhasil. */
export function setSession({ token, expiresAtUtc, user }) {
  authState.token = token
  authState.expiresAtUtc = expiresAtUtc
  authState.user = user
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ token, expiresAtUtc, user }))
}

export function clearSession() {
  authState.token = null
  authState.expiresAtUtc = null
  authState.user = null
  localStorage.removeItem(STORAGE_KEY)
}

restore()
