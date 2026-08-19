<script setup>
// Langkah kedua login: kode 6 digit dari authenticator app (TOTP, RFC 6238).
// Dua sumber tantangan MFA masuk ke halaman ini:
//  - Login manual (LoginPage.vue) -> disimpan di sessionStorage lewat services/mfaChallenge.js.
//  - Login Microsoft -> backend redirect ke sini dengan fragment "#challenge=<base64url>"
//    (fragment tidak pernah dikirim ke server maupun tercatat di log akses).
// Bila SetupRequired true, akun ini belum pernah mengaktifkan MFA: QR code + kunci
// manual ditampilkan dulu supaya user bisa scan sebelum memasukkan kode pertama.
import { computed, nextTick, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api } from '../services/api'
import { setSession } from '../services/auth'
import { getPendingMfaChallenge, setPendingMfaChallenge, clearPendingMfaChallenge } from '../services/mfaChallenge'
import { loadDashboard } from '../data/dashboard'
import { loadSheetManifest } from '../data/sheets'

const route = useRoute()
const router = useRouter()

const challenge = ref(null)
const digits = ref(['', '', '', '', '', ''])
const digitInputs = ref([])
const verifying = ref(false)
const error = ref('')

function decodePayload(value) {
  const b64 = value.replace(/-/g, '+').replace(/_/g, '/')
  const pad = b64.length % 4 ? '='.repeat(4 - (b64.length % 4)) : ''
  const binary = atob(b64 + pad)
  const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0))
  return JSON.parse(new TextDecoder().decode(bytes))
}

function bail(reason) {
  clearPendingMfaChallenge()
  router.replace({ name: 'login', query: { ssoError: reason } })
}

onMounted(() => {
  const hash = window.location.hash || ''
  const match = hash.match(/challenge=([^&]+)/)

  if (match) {
    try {
      const decoded = decodePayload(match[1])
      setPendingMfaChallenge(decoded)
      window.history.replaceState(null, '', window.location.pathname + window.location.search)
    } catch {
      bail('Data verifikasi MFA dari Microsoft tidak dapat dibaca.')
      return
    }
  }

  const pending = getPendingMfaChallenge()
  if (!pending?.challengeToken) {
    bail('Sesi login tidak ditemukan. Silakan login ulang.')
    return
  }

  challenge.value = pending
  nextTick(() => focusDigit(0))
})

const codeDigits = computed(() => digits.value.join(''))

function focusDigit(index) {
  digitInputs.value[index]?.focus()
}

function resetDigits() {
  digits.value = ['', '', '', '', '', '']
  nextTick(() => focusDigit(0))
}

// Satu digit per kotak: hanya angka terakhir yang diketik yang dipakai supaya
// pengetikan cepat atau autofill browser tidak meninggalkan lebih dari 1 karakter.
function onDigitInput(index, event) {
  const value = event.target.value.replace(/\D/g, '').slice(-1)
  digits.value[index] = value
  event.target.value = value

  if (value && index < 5) {
    focusDigit(index + 1)
  }
}

function onDigitKeydown(index, event) {
  if (event.key === 'Backspace' && !digits.value[index] && index > 0) {
    digits.value[index - 1] = ''
    focusDigit(index - 1)
  } else if (event.key === 'ArrowLeft' && index > 0) {
    focusDigit(index - 1)
  } else if (event.key === 'ArrowRight' && index < 5) {
    focusDigit(index + 1)
  }
}

// Mendukung paste kode 6 digit sekaligus (mis. dari password manager) ke kotak manapun.
function onPaste(event) {
  const text = event.clipboardData?.getData('text') ?? ''
  const pasted = text.replace(/\D/g, '').slice(0, 6)
  if (!pasted) return

  event.preventDefault()
  digits.value = Array.from({ length: 6 }, (_, i) => pasted[i] ?? '')
  focusDigit(Math.min(pasted.length, 5))
}

async function verify() {
  if (codeDigits.value.length !== 6) {
    error.value = 'Masukkan 6 digit kode dari aplikasi authenticator.'
    return
  }

  verifying.value = true
  error.value = ''
  try {
    const result = await api.post('/api/auth/mfa/verify', {
      challengeToken: challenge.value.challengeToken,
      code: codeDigits.value,
    })

    setSession(result)
    clearPendingMfaChallenge()

    // App sudah ter-mount tanpa data dashboard/manifest (endpointnya berpagar
    // token dan tadi belum ada sesi) - dimuat sekarang sebelum masuk ke halaman utama.
    try {
      await Promise.all([loadDashboard(), loadSheetManifest()])
    } catch {
      // Data awal boleh gagal dimuat di sini; halaman tujuan akan memuat ulang.
    }

    const returnUrl = route.query.returnUrl
    const dest = typeof returnUrl === 'string' && returnUrl && returnUrl !== '/login' ? returnUrl : '/'
    router.replace(dest)
  } catch (err) {
    error.value = err.message ?? 'Kode MFA salah.'
    resetDigits()
  } finally {
    verifying.value = false
  }
}

function backToLogin() {
  clearPendingMfaChallenge()
  router.replace({ name: 'login' })
}
</script>

<template>
  <div class="mfa-page">
    <div class="mfa-card" v-if="challenge">
      <span class="mfa-mark">R4</span>

      <template v-if="challenge.setupRequired">
        <h1 class="mfa-title">Aktifkan Autentikator</h1>
        <p class="mfa-subtitle">
          Akun ini belum mengaktifkan MFA. Scan QR code di bawah memakai Google Authenticator,
          Microsoft Authenticator, atau aplikasi TOTP lainnya, lalu masukkan kode 6 digit yang muncul.
        </p>

        <div class="mfa-qr">
          <img :src="challenge.qrCodeDataUri" alt="QR code setup MFA" />
        </div>

        <details class="mfa-manual">
          <summary>Tidak bisa scan? Masukkan kunci manual</summary>
          <code class="mfa-manual__key">{{ challenge.manualEntryKey }}</code>
        </details>
      </template>

      <template v-else>
        <h1 class="mfa-title">Verifikasi Dua Langkah</h1>
        <p class="mfa-subtitle">
          Masukkan kode 6 digit dari aplikasi authenticator yang terpasang di akun ini.
        </p>
      </template>

      <form @submit.prevent="verify">
        <p v-if="error" class="mfa-error">{{ error }}</p>

        <div class="mfa-code-boxes">
          <input
            v-for="(digit, i) in digits"
            :key="i"
            ref="digitInputs"
            class="mfa-code-box"
            :value="digit"
            @input="onDigitInput(i, $event)"
            @keydown="onDigitKeydown(i, $event)"
            @paste="onPaste"
            inputmode="numeric"
            autocomplete="one-time-code"
            maxlength="1"
          />
        </div>

        <button type="submit" class="btn-submit" :disabled="verifying || codeDigits.length !== 6">
          {{ verifying ? 'Memverifikasi…' : 'Verifikasi' }}
        </button>
      </form>

      <button type="button" class="mfa-back" @click="backToLogin">Kembali ke halaman login</button>
    </div>
  </div>
</template>

<style scoped>
.mfa-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1.5rem;
  background: var(--page-bg);
}

.mfa-card {
  width: 100%;
  max-width: 400px;
  background: var(--surface);
  border-radius: 18px;
  box-shadow: 0 20px 50px rgba(10, 18, 50, 0.14);
  padding: 2.25rem 2rem;
  text-align: center;
}

.mfa-mark {
  display: inline-grid;
  place-items: center;
  width: 44px;
  height: 44px;
  margin: 0 auto 1.1rem;
  border-radius: 12px;
  background: linear-gradient(135deg, #d93025, #f0b429);
  font-weight: 800;
  font-size: 1rem;
  color: #fff;
}

.mfa-title {
  font-size: 1.35rem;
  font-weight: 800;
  color: var(--ink-strong);
  margin-bottom: 0.5rem;
}

.mfa-subtitle {
  font-size: 0.82rem;
  color: var(--ink-muted);
  line-height: 1.5;
  margin-bottom: 1.5rem;
}

.mfa-qr {
  display: flex;
  justify-content: center;
  margin-bottom: 1.25rem;
}

.mfa-qr img {
  width: 176px;
  height: 176px;
  border-radius: 12px;
  border: 1px solid var(--line);
  padding: 0.5rem;
  background: #fff;
}

.mfa-manual {
  margin-bottom: 1.5rem;
  font-size: 0.78rem;
  color: var(--ink-muted);
}

.mfa-manual summary {
  cursor: pointer;
  font-weight: 600;
  color: var(--accent-blue);
}

.mfa-manual__key {
  display: block;
  margin-top: 0.6rem;
  padding: 0.5rem 0.65rem;
  background: var(--page-bg);
  border: 1px solid var(--line);
  border-radius: 8px;
  font-family: ui-monospace, SFMono-Regular, Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 0.7rem;
  letter-spacing: 0.02em;
  white-space: nowrap;
  overflow-x: auto;
}

.mfa-error {
  background: #fdecea;
  color: var(--accent-red);
  border: 1px solid #f5c6c0;
  border-radius: 8px;
  padding: 0.5rem 0.75rem;
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 1rem;
  text-align: left;
}

.mfa-code-boxes {
  display: flex;
  justify-content: center;
  gap: 0.6rem;
  margin-bottom: 1.25rem;
}

.mfa-code-box {
  width: 44px;
  height: 52px;
  border: 1px solid var(--line);
  border-radius: 10px;
  background: var(--page-bg);
  color: var(--ink-strong);
  font-size: 1.4rem;
  font-weight: 700;
  text-align: center;
  padding: 0;
}

.mfa-code-box:focus {
  outline: none;
  border-color: var(--accent-blue);
  box-shadow: 0 0 0 3px rgba(29, 64, 176, 0.15);
}

@media (max-width: 359.98px) {
  .mfa-code-boxes {
    gap: 0.4rem;
  }

  .mfa-code-box {
    width: 38px;
    height: 46px;
    font-size: 1.2rem;
  }
}

.btn-submit {
  width: 100%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  border: none;
  border-radius: 10px;
  padding: 0.7rem 1rem;
  background: linear-gradient(135deg, var(--accent-blue), #2b52d6);
  color: #fff;
  font-weight: 700;
  font-size: 0.9rem;
  box-shadow: 0 10px 24px rgba(29, 64, 176, 0.3);
  transition: filter 0.15s;
}

.btn-submit:hover:not(:disabled) {
  filter: brightness(1.06);
}

.btn-submit:disabled {
  opacity: 0.6;
  cursor: default;
}

.mfa-back {
  display: block;
  margin: 1.25rem auto 0;
  background: none;
  border: none;
  padding: 0;
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--accent-blue);
}

.mfa-back:hover {
  text-decoration: underline;
}
</style>
