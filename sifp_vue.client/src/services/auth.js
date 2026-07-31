// Status login klien: token JWT + profil user.
//
// Dashboard dan seluruh halaman master bisa dibuka tanpa login (endpoint bacanya
// terbuka). Login hanya diperlukan untuk aksi tulis — saat ini import Excel.

import { computed, reactive } from 'vue'

const STORAGE_KEY = 'sifp.auth'

function readStored() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null

    const parsed = JSON.parse(raw)
    // Token kedaluwarsa dibuang saat start supaya UI tidak sempat menampilkan
    // status "sudah login" yang sebenarnya sudah tidak berlaku.
    if (!parsed?.token || (parsed.expiresAtUtc && new Date(parsed.expiresAtUtc) <= new Date())) {
      localStorage.removeItem(STORAGE_KEY)
      return null
    }
    return parsed
  } catch {
    return null
  }
}

const stored = readStored()

const state = reactive({
  token: stored?.token ?? null,
  expiresAtUtc: stored?.expiresAtUtc ?? null,
  user: stored?.user ?? null,
})

export const session = state

export const isLoggedIn = computed(() => Boolean(state.token))
export const currentUser = computed(() => state.user)
export const userRoles = computed(() => state.user?.roles ?? [])

/** True bila user boleh melakukan import (Administrator atau Verifier). */
export const canImport = computed(() =>
  userRoles.value.some((r) => r === 'Administrator' || r === 'Verifier')
)

export function authHeader() {
  return state.token ? { Authorization: `Bearer ${state.token}` } : {}
}

export function setSession({ token, expiresAtUtc, user }) {
  state.token = token
  state.expiresAtUtc = expiresAtUtc ?? null
  state.user = user ?? null
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ token, expiresAtUtc, user }))
}

export function clearSession() {
  state.token = null
  state.expiresAtUtc = null
  state.user = null
  localStorage.removeItem(STORAGE_KEY)
}

/**
 * Login ke backend. Diimpor secara dinamis untuk memutus siklus impor
 * antara api.js (butuh authHeader) dan modul ini (butuh api.post).
 */
export async function login(username, password) {
  const { api } = await import('./api')
  const data = await api.post('/api/auth/login', { username, password })
  setSession(data)
  return data.user
}

export function logout() {
  clearSession()
}
