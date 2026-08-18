<script setup>
// Halaman transit setelah login Microsoft berhasil. Backend meng-encode hasil
// login (token + user) sebagai fragment URL "#sso=<base64url>". Fragment tidak
// pernah dikirim ke server maupun tercatat di log akses, jadi aman untuk token.
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { setSession } from '../services/auth'
import { loadDashboard } from '../data/dashboard'
import { loadSheetManifest } from '../data/sheets'

const route = useRoute()
const router = useRouter()
const message = ref('Menyelesaikan login Microsoft…')

function decodePayload(value) {
  const b64 = value.replace(/-/g, '+').replace(/_/g, '/')
  const pad = b64.length % 4 ? '='.repeat(4 - (b64.length % 4)) : ''
  const binary = atob(b64 + pad)
  const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0))
  return JSON.parse(new TextDecoder().decode(bytes))
}

function bail(reason) {
  router.replace({ name: 'login', query: { ssoError: reason } })
}

onMounted(async () => {
  const hash = window.location.hash || ''
  const match = hash.match(/sso=([^&]+)/)
  if (!match) {
    bail('Login Microsoft tidak mengembalikan sesi yang valid.')
    return
  }

  let result
  try {
    result = decodePayload(match[1])
  } catch {
    bail('Data sesi dari Microsoft tidak dapat dibaca.')
    return
  }

  if (!result?.token) {
    bail('Token login Microsoft tidak ditemukan.')
    return
  }

  setSession(result)

  // Hapus fragment berisi token dari address bar sebelum melanjutkan.
  window.history.replaceState(null, '', window.location.pathname)

  try {
    await Promise.all([loadDashboard(), loadSheetManifest()])
  } catch {
    // Data awal boleh gagal dimuat di sini; halaman tujuan akan memuat ulang.
  }

  const returnUrl = route.query.returnUrl
  const dest = typeof returnUrl === 'string' && returnUrl && returnUrl !== '/login' ? returnUrl : '/'
  router.replace(dest)
})
</script>

<template>
  <div class="auth-callback">
    <div class="auth-callback__spinner" aria-hidden="true"></div>
    <p class="auth-callback__text">{{ message }}</p>
  </div>
</template>

<style scoped>
.auth-callback {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  background: var(--page-bg);
  color: var(--ink-muted);
}

.auth-callback__spinner {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  border: 3px solid var(--line);
  border-top-color: var(--accent-blue);
  animation: auth-spin 0.8s linear infinite;
}

.auth-callback__text {
  font-size: 0.9rem;
  font-weight: 600;
}

@keyframes auth-spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
