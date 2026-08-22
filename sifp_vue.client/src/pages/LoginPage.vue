<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DashIcon from '../components/dashboard/DashIcon.vue'
import { api } from '../services/api'
import { setPendingMfaChallenge } from '../services/mfaChallenge'
import loginVisual from '../assets/images/d6a6296b-e298-48ef-a171-cfdd2b034f30.png'
import iogLogo from '../assets/images/IOG PNG.png'
import pertamina68Logo from '../assets/images/Pertamina Logo 68 Final-Logo Alternatif.png'
import skkLogo from '../assets/images/SKK.png'

const route = useRoute()
const router = useRouter()

const username = ref('')
const password = ref('')
const rememberMe = ref(true)
const showPassword = ref(false)
const showForgotNotice = ref(false)

const manualLoading = ref(false)
const windowsLoading = ref(false)
const ssoError = ref('')
const manualError = ref('')

const year = new Date().getFullYear()

// Backend mengembalikan kegagalan SSO sebagai query ?ssoError=... di halaman login.
onMounted(() => {
  const err = route.query.ssoError
  if (typeof err === 'string' && err) {
    ssoError.value = err
  }
})

const passwordFieldType = computed(() => (showPassword.value ? 'text' : 'password'))

function destination() {
  const returnUrl = route.query.returnUrl
  return typeof returnUrl === 'string' && returnUrl && returnUrl !== '/login' ? returnUrl : '/'
}

async function loginWithWindows() {
  windowsLoading.value = true
  ssoError.value = ''
  try {
    const challenge = await api.post('/api/auth/windows')
    setPendingMfaChallenge(challenge)
    router.push({ name: 'mfa', query: { returnUrl: destination() } })
  } catch (err) {
    ssoError.value = err.message || 'Login Windows Authenticator gagal.'
  } finally {
    windowsLoading.value = false
  }
}

async function loginManual() {
  manualLoading.value = true
  manualError.value = ''
  try {
    // Username/password benar hanya membuka tantangan MFA; sesi baru aktif
    // setelah kode 6 digit diverifikasi di halaman /mfa (lihat MfaPage.vue).
    const challenge = await api.post('/api/auth/login', {
      username: username.value.trim(),
      password: password.value,
      rememberMe: rememberMe.value,
    })
    setPendingMfaChallenge(challenge)
    router.push({ name: 'mfa', query: { returnUrl: destination() } })
  } catch (err) {
    manualError.value = err.message ?? 'Login gagal.'
  } finally {
    manualLoading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-visual">
      <img :src="loginVisual" alt="" class="login-visual__image" />
      <div class="login-visual__scrim" aria-hidden="true"></div>

      <div class="login-visual__content">
        <div class="login-visual__copy">
          <span class="login-visual__eyebrow">Regional 4 · Pertamina EP Cepu</span>
          <h1 class="login-headline">Drive Assurance.<br /><span>Deliver Excellence.</span></h1>
          <p class="login-subtext">
            Monitoring, improving, and assuring operational excellence across every SIFP exposure in Regional 4.
          </p>
        </div>

        <div class="login-visual__partners">
          <span class="login-visual__partners-label">Selaras dengan program nasional</span>
          <div class="login-visual__logos">
            <span class="login-visual__logo-chip">
              <img :src="skkLogo" alt="SKK Migas" />
            </span>
            <span class="login-visual__logo-chip">
              <img :src="iogLogo" alt="IOG 4.0" />
            </span>
            <span class="login-visual__logo-chip">
              <img :src="pertamina68Logo" alt="Pertamina 68 - Energizing Indonesia" />
            </span>
          </div>
        </div>
      </div>
    </div>

    <div class="login-form-panel">
      <div class="login-form-panel__decor" aria-hidden="true"></div>

      <div class="login-card">
        <span class="login-mark login-mark--center">R4</span>
        <h2 class="login-title">Welcome Back</h2>
        <p class="login-subtitle">Sign in to continue to SIFP Assurance Dashboard</p>

        <button type="button" class="btn-windows btn-windows-auth" :disabled="windowsLoading" @click="loginWithWindows">
          <DashIcon name="shield" :size="16" />
          {{ windowsLoading ? 'Memproses Windows Auth…' : 'Sign in with Windows Authenticator' }}
        </button>
        <p v-if="ssoError" class="login-error">{{ ssoError }}</p>

        <div class="login-divider"><span>OR</span></div>

        <form @submit.prevent="loginManual">
          <p v-if="manualError" class="login-error">{{ manualError }}</p>

          <div class="login-field mb-3">
            <label for="login-username" class="form-label">Username</label>
            <div class="login-field__control">
              <DashIcon name="person" :size="16" class="login-field__icon" />
              <input
                id="login-username"
                v-model="username"
                type="text"
                class="form-control"
                autocomplete="username"
                placeholder="Masukkan username"
                required
              />
            </div>
          </div>

          <div class="login-field mb-3">
            <label for="login-password" class="form-label">Password</label>
            <div class="login-field__control">
              <DashIcon name="lock" :size="16" class="login-field__icon" />
              <input
                id="login-password"
                v-model="password"
                :type="passwordFieldType"
                class="form-control"
                autocomplete="current-password"
                placeholder="Masukkan password"
                required
              />
              <button
                type="button"
                class="login-field__toggle"
                :aria-label="showPassword ? 'Sembunyikan password' : 'Tampilkan password'"
                @click="showPassword = !showPassword"
              >
                <DashIcon :name="showPassword ? 'eye-off' : 'eye'" :size="16" />
              </button>
            </div>
          </div>

          <div class="login-row">
            <label class="login-remember">
              <input type="checkbox" v-model="rememberMe" />
              Remember me
            </label>
            <button type="button" class="login-forgot" @click="showForgotNotice = !showForgotNotice">
              Forgot password?
            </button>
          </div>
          <p v-if="showForgotNotice" class="login-forgot-notice">
            Hubungi administrator untuk reset password.
          </p>

          <button type="submit" class="btn-submit" :disabled="manualLoading">
            {{ manualLoading ? 'Memproses…' : 'Sign In' }}
            <DashIcon name="arrow-right" :size="18" />
          </button>
        </form>

        <p class="login-secure">
          <DashIcon name="lock" :size="13" /> Secure access · Protected by Pertamina EP Cepu
        </p>
      </div>

      <p class="login-footer">
        © {{ year }} Pertamina EP Cepu · Regional 4 SIFP Assurance<br />All rights reserved
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
}

/* ---------- Left: brand / marketing panel ---------- */

.login-visual {
  position: relative;
  flex: 1 1 54%;
  overflow: hidden;
  background: #0a1233;
}

.login-visual__image {
  width: 100%;
  height: 100%;
  display: block;
  object-fit: cover;
  object-position: left center;
}

.login-visual__scrim {
  position: absolute;
  inset: 0;
  background: linear-gradient(115deg, rgba(6, 12, 40, 0.05) 28%, rgba(6, 12, 40, 0.58) 62%, rgba(6, 12, 40, 0.85) 100%);
  pointer-events: none;
}

.login-visual__content {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: flex-end;
  gap: 2.5rem;
  padding: 3rem;
  color: #fff;
}

.login-visual__copy,
.login-visual__partners {
  max-width: 400px;
  text-align: left;
}

.login-visual__eyebrow {
  display: inline-block;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: #9db4ff;
  margin-bottom: 0.9rem;
}

.login-headline {
  font-size: clamp(1.7rem, 2.6vw, 2.35rem);
  font-weight: 800;
  line-height: 1.22;
  margin-bottom: 0.9rem;
}

.login-headline span {
  color: #7fa1ff;
}

.login-subtext {
  font-size: 0.9rem;
  color: rgba(255, 255, 255, 0.82);
  line-height: 1.55;
}

.login-visual__partners-label {
  display: block;
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 0.75rem;
}

.login-visual__logos {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  flex-wrap: wrap;
}

.login-visual__logo-chip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  height: 46px;
  padding: 0.45rem 0.7rem;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 8px 20px rgba(4, 10, 35, 0.35);
}

.login-visual__logo-chip img {
  height: 100%;
  width: auto;
  max-width: 88px;
  object-fit: contain;
  display: block;
}

.login-mark {
  flex: none;
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: linear-gradient(135deg, #d93025, #f0b429);
  font-weight: 800;
  font-size: 1rem;
  color: #fff;
}

/* ---------- Right: form panel ---------- */

.login-form-panel {
  position: relative;
  flex: 1 1 46%;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 2.5rem 1.5rem;
  background: var(--page-bg);
}

.login-form-panel__decor {
  position: absolute;
  inset: -20% -10% auto auto;
  width: 420px;
  height: 420px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(29, 64, 176, 0.1), transparent 70%);
  pointer-events: none;
}

.login-card {
  position: relative;
  width: 100%;
  max-width: 400px;
  background: var(--surface);
  border-radius: 18px;
  box-shadow: 0 20px 50px rgba(10, 18, 50, 0.14);
  padding: 2.25rem 2rem;
  text-align: center;
}

.login-mark--center {
  margin: 0 auto 1.1rem;
}

.login-title {
  font-size: 1.35rem;
  font-weight: 800;
  color: var(--ink-strong);
  margin-bottom: 0.3rem;
}

.login-subtitle {
  font-size: 0.82rem;
  color: var(--ink-muted);
  margin-bottom: 1.5rem;
}

.login-card form {
  text-align: left;
}

.btn-windows {
  width: 100%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  padding: 0.65rem 1rem;
  border-radius: 10px;
  border: 1px solid var(--line);
  background: var(--surface);
  color: var(--ink-strong);
  font-weight: 700;
  font-size: 0.86rem;
  transition: background 0.15s, border-color 0.15s;
  cursor: pointer;
}

.btn-windows:hover:not(:disabled) {
  background: #eef1f9;
  border-color: var(--accent-blue);
}

.btn-windows:disabled {
  opacity: 0.65;
  cursor: default;
}

.btn-windows-auth {
  background: #f0f4ff;
  border-color: #d0dcfb;
  color: var(--accent-blue);
}

.btn-windows-auth:hover:not(:disabled) {
  background: #e2ebff;
  border-color: var(--accent-blue);
}

.login-divider {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin: 1.25rem 0;
  color: var(--ink-muted);
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.login-divider::before,
.login-divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--line);
}

.login-error {
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

.login-field .form-label {
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--ink-strong);
}

.login-field__control {
  position: relative;
  display: flex;
  align-items: center;
}

.login-field__icon {
  position: absolute;
  left: 0.75rem;
  color: var(--ink-muted);
  pointer-events: none;
}

.login-field__control .form-control {
  padding-left: 2.15rem;
}

.login-field__toggle {
  position: absolute;
  right: 0.6rem;
  background: none;
  border: none;
  color: var(--ink-muted);
  display: inline-flex;
  padding: 0.2rem;
}

.login-field__toggle:hover {
  color: var(--accent-blue);
}

.login-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.4rem;
}

.login-remember {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--ink);
}

.login-remember input {
  accent-color: var(--accent-blue);
}

.login-forgot {
  background: none;
  border: none;
  padding: 0;
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--accent-blue);
}

.login-forgot:hover {
  text-decoration: underline;
}

.login-forgot-notice {
  font-size: 0.74rem;
  color: var(--ink-muted);
  margin-bottom: 0.75rem;
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
  margin-top: 1.4rem;
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
  opacity: 0.7;
  cursor: default;
}

.login-secure {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  margin: 1.5rem 0 0;
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.login-footer {
  position: relative;
  text-align: center;
  font-size: 0.68rem;
  color: var(--ink-muted);
  line-height: 1.5;
}

@media (max-width: 991.98px) {
  .login-visual {
    display: none;
  }

  .login-form-panel {
    flex: 1 1 100%;
  }
}
</style>
