<script setup>
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DashIcon from '../components/dashboard/DashIcon.vue'
import { api } from '../services/api'
import { setSession } from '../services/auth'
import { loadDashboard } from '../data/dashboard'
import { loadSheetManifest } from '../data/sheets'

const route = useRoute()
const router = useRouter()

const username = ref('')
const password = ref('')
const rememberMe = ref(true)
const showPassword = ref(false)
const showForgotNotice = ref(false)

const windowsLoading = ref(false)
const manualLoading = ref(false)
const windowsError = ref('')
const manualError = ref('')

const year = new Date().getFullYear()

const features = [
  { icon: 'refresh', title: 'Real-time Monitoring', text: 'Live data & insights' },
  { icon: 'shield', title: 'Risk Focused', text: 'Prioritize what matters' },
  { icon: 'auto', title: 'Data Driven', text: 'Actionable intelligence' },
  { icon: 'gear', title: 'Operational Excellence', text: 'Continuous improvement' },
]

const passwordFieldType = computed(() => (showPassword.value ? 'text' : 'password'))

function destination() {
  const returnUrl = route.query.returnUrl
  return typeof returnUrl === 'string' && returnUrl && returnUrl !== '/login' ? returnUrl : '/'
}

async function afterLogin(result) {
  setSession(result)
  // App sudah ter-mount tanpa data dashboard/manifest (endpointnya berpagar token
  // dan tadi belum ada sesi) - dimuat sekarang sebelum masuk ke halaman utama.
  await Promise.all([loadDashboard(), loadSheetManifest()])
  router.push(destination())
}

async function loginWithWindows() {
  windowsLoading.value = true
  windowsError.value = ''
  try {
    const result = await api.get('/api/auth/windows')
    await afterLogin(result)
  } catch (err) {
    windowsError.value = err.message ?? 'Login Windows gagal.'
  } finally {
    windowsLoading.value = false
  }
}

async function loginManual() {
  manualLoading.value = true
  manualError.value = ''
  try {
    const result = await api.post('/api/auth/login', {
      username: username.value.trim(),
      password: password.value,
      rememberMe: rememberMe.value,
    })
    await afterLogin(result)
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
      <div class="login-visual__bg" aria-hidden="true"></div>

      <div class="login-visual__content">
        <div class="login-brand">
          <span class="login-mark">R4</span>
          <span class="login-brand-text">
            <strong>SIFP ASSURANCE</strong>
            <small>Regional 4 · Pertamina EP Cepu</small>
          </span>
        </div>

        <h1 class="login-headline">Drive Assurance.<br /><span>Deliver Excellence.</span></h1>
        <p class="login-subtext">
          Monitoring, improving, and assuring operational excellence across Regional 4.
        </p>

        <div class="login-preview" aria-hidden="true">
          <div class="login-preview__bar">
            <span></span><span></span><span></span>
          </div>
          <div class="login-preview__body">
            <div class="login-preview__gauges">
              <div class="login-preview__gauge" style="--val: 70%; --gauge-color: var(--st-effective)">
                <span>70%</span>
              </div>
              <div class="login-preview__gauge" style="--val: 90%; --gauge-color: var(--accent-blue)">
                <span>90%</span>
              </div>
              <div class="login-preview__gauge" style="--val: 57%; --gauge-color: var(--accent-red)">
                <span>57%</span>
              </div>
            </div>
            <div class="login-preview__rows">
              <span class="login-preview__row" v-for="n in 4" :key="n">
                <i class="login-preview__row-dot" :class="`is-${n}`"></i>
                <i class="login-preview__row-bar"></i>
              </span>
            </div>
          </div>
        </div>

        <div class="login-features">
          <div class="login-feature" v-for="feature in features" :key="feature.title">
            <span class="login-feature__icon"><DashIcon :name="feature.icon" :size="18" /></span>
            <span class="login-feature__text">
              <strong>{{ feature.title }}</strong>
              <small>{{ feature.text }}</small>
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

        <button type="button" class="btn-windows" :disabled="windowsLoading" @click="loginWithWindows">
          <svg class="windows-icon" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
            <rect x="0" y="0" width="7" height="7" fill="currentColor" />
            <rect x="8" y="0" width="8" height="7" fill="currentColor" />
            <rect x="0" y="8" width="7" height="8" fill="currentColor" />
            <rect x="8" y="8" width="8" height="8" fill="currentColor" />
          </svg>
          {{ windowsLoading ? 'Checking Windows account…' : 'Sign in with Windows Account' }}
        </button>
        <p v-if="windowsError" class="login-error">{{ windowsError }}</p>

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
  background: linear-gradient(160deg, #14246b, #1e2f83 55%, #24338f);
  padding: 3rem 3.25rem;
  display: flex;
  align-items: center;
}

.login-visual__bg {
  position: absolute;
  inset: 0;
  background-image: radial-gradient(rgba(255, 255, 255, 0.16) 1.3px, transparent 1.3px);
  background-size: 16px 16px;
  -webkit-mask-image: radial-gradient(ellipse 65% 60% at 68% 45%, #000 40%, transparent 78%);
  mask-image: radial-gradient(ellipse 65% 60% at 68% 45%, #000 40%, transparent 78%);
}

.login-visual__content {
  position: relative;
  max-width: 620px;
  margin: 0 auto;
  color: #fff;
}

.login-brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 2.25rem;
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

.login-brand-text {
  display: flex;
  flex-direction: column;
  line-height: 1.25;
}

.login-brand-text strong {
  font-size: 0.98rem;
  font-weight: 800;
  letter-spacing: 0.03em;
}

.login-brand-text small {
  font-size: 0.68rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.7);
}

.login-headline {
  font-size: clamp(1.8rem, 3vw, 2.5rem);
  font-weight: 800;
  line-height: 1.2;
  margin-bottom: 1rem;
}

.login-headline span {
  color: #7fa1ff;
}

.login-subtext {
  font-size: 0.95rem;
  color: rgba(255, 255, 255, 0.78);
  max-width: 420px;
  margin-bottom: 2rem;
}

/* Mock dashboard preview card -- decorative only, not real data. */
.login-preview {
  background: var(--surface);
  border-radius: 14px;
  box-shadow: 0 24px 48px rgba(6, 12, 40, 0.4);
  overflow: hidden;
  margin-bottom: 2.25rem;
}

.login-preview__bar {
  display: flex;
  gap: 0.3rem;
  padding: 0.6rem 0.75rem;
  border-bottom: 1px solid var(--line);
}

.login-preview__bar span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--line);
}

.login-preview__body {
  padding: 1.35rem 1.4rem 1.5rem;
}

.login-preview__gauges {
  display: flex;
  gap: 1.4rem;
  margin-bottom: 1.4rem;
}

.login-preview__gauge {
  position: relative;
  width: 72px;
  height: 72px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  background: conic-gradient(var(--gauge-color) var(--val), #e3e7f3 0);
}

.login-preview__gauge::before {
  content: '';
  position: absolute;
  inset: 9px;
  border-radius: 50%;
  background: var(--surface);
}

.login-preview__gauge span {
  position: relative;
  font-size: 0.92rem;
  font-weight: 800;
  color: var(--gauge-color);
}

.login-preview__row {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin-top: 0.75rem;
}

.login-preview__row-dot {
  flex: none;
  width: 9px;
  height: 9px;
  border-radius: 50%;
}

.login-preview__row-dot.is-1,
.login-preview__row-dot.is-4 {
  background: var(--st-effective);
}

.login-preview__row-dot.is-2 {
  background: var(--st-degraded);
}

.login-preview__row-dot.is-3 {
  background: var(--st-failed);
}

.login-preview__row-bar {
  flex: 1;
  height: 9px;
  border-radius: 5px;
  background: #e9ecf6;
}

.login-features {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem 1.5rem;
}

.login-feature {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}

.login-feature__icon {
  flex: none;
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.14);
  color: #9db4ff;
}

.login-feature__text {
  display: flex;
  flex-direction: column;
  line-height: 1.3;
}

.login-feature__text strong {
  font-size: 0.8rem;
  font-weight: 700;
}

.login-feature__text small {
  font-size: 0.7rem;
  color: rgba(255, 255, 255, 0.65);
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
}

.btn-windows:hover:not(:disabled) {
  background: #eef1f9;
  border-color: var(--accent-blue);
}

.btn-windows:disabled {
  opacity: 0.65;
  cursor: default;
}

.windows-icon {
  color: var(--accent-blue);
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
