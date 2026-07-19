import { createRouter, createWebHistory } from 'vue-router'

const routes = [
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
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

router.afterEach((to) => {
  document.title = `${to.meta.title} · SIFP Assurance`
})

export default router
