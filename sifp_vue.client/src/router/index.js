import { createRouter, createWebHistory } from 'vue-router'
import { sheetBySlug } from '../data/sheets'
import { isTokenValid } from '../services/auth'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../pages/LoginPage.vue'),
    meta: { title: 'Masuk', subtitle: 'SIFP Assurance', public: true },
  },
  {
    // Langkah kedua login (kode 6 digit authenticator app) — dibuka dari
    // LoginPage.vue setelah password benar, atau lewat redirect Microsoft
    // (login/callback SSO juga wajib lewat MFA, lihat MicrosoftAuthController).
    path: '/auth/mfa',
    name: 'mfa',
    component: () => import('../pages/MfaPage.vue'),
    meta: { title: 'Verifikasi MFA', subtitle: 'SIFP Assurance', public: true },
  },
  {
    path: '/',
    name: 'dashboard',
    component: () => import('../pages/DashboardPage.vue'),
    // dashboard butuh ruang horizontal maksimal -> sidebar default mode ikon
    meta: { title: 'Dashboard', subtitle: 'Regional 4 SIFP Assurance', collapseSidebar: true },
  },
  {
    path: '/import',
    name: 'import',
    component: () => import('../pages/ImportPage.vue'),
    meta: { title: 'Import Excel', subtitle: 'Data Management' },
  },
  {
    path: '/master/observations',
    name: 'observations',
    component: () => import('../pages/master/ObservationsPage.vue'),
    meta: { title: 'Observations', subtitle: 'Master Data' },
  },
  {
    path: '/master/sif-questions',
    name: 'sif-questions',
    component: () => import('../pages/master/SifQuestionsPage.vue'),
    meta: { title: 'SIF Questions', subtitle: 'Master Data' },
  },
  {
    path: '/master/ccvc-library',
    name: 'ccvc-library',
    component: () => import('../pages/master/CcvcLibraryPage.vue'),
    meta: { title: 'PSEC & CCVC Library', subtitle: 'Master Data' },
  },
  {
    path: '/master/error-traps',
    name: 'error-traps',
    component: () => import('../pages/master/ErrorTrapsPage.vue'),
    meta: { title: 'Error Traps', subtitle: 'Master Data' },
  },
  {
    path: '/master/hp-tools',
    name: 'hp-tools',
    component: () => import('../pages/master/HpToolsPage.vue'),
    meta: { title: 'HP Tools', subtitle: 'Master Data' },
  },
  {
    path: '/master/drift-conditions',
    name: 'drift-conditions',
    component: () => import('../pages/master/DriftConditionsPage.vue'),
    meta: { title: 'Drift Conditions', subtitle: 'Master Data' },
  },
  {
    path: '/master/latent-conditions',
    name: 'latent-conditions',
    component: () => import('../pages/master/LatentConditionsPage.vue'),
    meta: { title: 'Latent Conditions', subtitle: 'Master Data' },
  },
  {
    path: '/master/initiatives',
    name: 'initiatives',
    component: () => import('../pages/master/InitiativesPage.vue'),
    meta: { title: 'Improvement Initiatives', subtitle: 'Master Data' },
  },
  {
    // Viewer generik untuk worksheet yang belum punya halaman kurasi.
    path: '/sheet/:slug',
    name: 'sheet',
    component: () => import('../pages/SheetViewerPage.vue'),
    meta: { title: 'Worksheet', subtitle: 'Data Excel' },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

// Tanpa token yang masih berlaku, semua route selain /login diarahkan ke sana;
// sebaliknya /login tidak boleh dibuka lagi kalau sesi masih valid.
router.beforeEach((to) => {
  const authed = isTokenValid()

  if (to.meta.public) {
    if (authed) return { name: 'dashboard' }
    return true
  }

  if (!authed) {
    return { name: 'login', query: { returnUrl: to.fullPath } }
  }

  return true
})

// Route worksheet memakai satu definisi untuk semua sheet, jadi judul topbar
// diisi dari manifest sesuai slug yang sedang dibuka.
router.beforeEach((to) => {
  if (to.name === 'sheet') {
    const entry = sheetBySlug[to.params.slug]
    to.meta.title = entry?.label ?? 'Worksheet'
    to.meta.subtitle = entry ? `Worksheet · ${entry.name}` : 'Data Excel'
  }
})

router.afterEach((to) => {
  document.title = `${to.meta.title} · SIFP Assurance`
})

export default router
