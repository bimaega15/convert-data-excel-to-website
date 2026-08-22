<script setup>
import DashboardHeader from '../components/dashboard/DashboardHeader.vue'
import GaugeCard from '../components/dashboard/GaugeCard.vue'
import ConformanceCard from '../components/dashboard/ConformanceCard.vue'
import ZoneScoresCard from '../components/dashboard/ZoneScoresCard.vue'
import QuickFacts from '../components/dashboard/QuickFacts.vue'
import HealthMap from '../components/dashboard/HealthMap.vue'
import TopFivePanel from '../components/dashboard/TopFivePanel.vue'
import TrendChart from '../components/dashboard/TrendChart.vue'
import ZonaChart from '../components/dashboard/ZonaChart.vue'
import InitiativesTable from '../components/dashboard/InitiativesTable.vue'
import ExecutiveSummary from '../components/dashboard/ExecutiveSummary.vue'
import DashIcon from '../components/dashboard/DashIcon.vue'
import { kpis, topPanels, footerNote } from '../data/dashboard'
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue'
import html2canvas from 'html2canvas'

const scale = ref(1)
const isAutoFit = ref(false)
const contentHeight = ref(1200)
const contentWidth = ref(1760)
const dashRef = ref(null)
const dashWrapperRef = ref(null)
const showSettings = ref(false)
const isCapturing = ref(false)

const defaultVisibility = {
  psec: true,
  ccvc: true,
  psie: true,
  conformance: true,
  zoneScores: true,
  quickFacts: true,
  healthMap: true,
  topPanel1: true,
  topPanel2: true,
  topPanel3: true,
  topPanel4: true,
  trendChart: true,
  zonaChart: true,
  initiatives: true,
  executiveSummary: true
}

const visibleCards = ref(JSON.parse(localStorage.getItem('dashboardVisibility')) || defaultVisibility)

// Computed untuk mengecek apakah setiap baris memiliki minimal 1 card yang visible
// sehingga baris kosong dihapus dari DOM (tidak ada ruang kosong saat di-capture)
const showKpiRow = computed(() =>
  visibleCards.value.psec ||
  visibleCards.value.ccvc ||
  visibleCards.value.psie ||
  visibleCards.value.conformance ||
  visibleCards.value.zoneScores
)

// Baris "blok 1-7": Health Map (tinggi, kiri) + 2 sub-baris Top 5/Trend/Tabel di kanannya,
// meniru layout slide (bagian kanan punya 2 baris yang tingginya gabung menyamai Health Map).
const showTop5Top = computed(() =>
  visibleCards.value.topPanel1 || visibleCards.value.topPanel2 || visibleCards.value.topPanel3
)

const showTop5Bottom = computed(() =>
  visibleCards.value.topPanel4 || visibleCards.value.trendChart || visibleCards.value.zonaChart
)

const showMidRow = computed(() =>
  visibleCards.value.healthMap || showTop5Top.value || showTop5Bottom.value
)

// Baris "blok 8-9": Initiatives + Executive Summary berdampingan, masing-masing separuh lebar.
const showBottomRow = computed(() =>
  visibleCards.value.initiatives || visibleCards.value.executiveSummary
)

watch(visibleCards, (newVal) => {
  localStorage.setItem('dashboardVisibility', JSON.stringify(newVal))
  // Recalculate scale whenever visibility changes, as content height might change
  if (isAutoFit.value) {
    nextTick(() => calculateScale())
  }
}, { deep: true })

// Mengukur ulang lebar & tinggi asli konten (pada scale berapa pun, karena
// transform:scale() tidak mempengaruhi layout box).
// PENTING: jangan ambil lebar dari dashRef.clientWidth, karena elemen itu
// berada di dalam .dash-scaler yang ukurannya sendiri dihitung dari
// contentWidth*scale — itu akan membuat loop yang mengecilkan/membesarkan
// contentWidth setiap kali di-resize/di-measure.
// Padding wrapper (muncul saat mode manual/is-scrollable) dikurangi secara
// eksplisit karena clientWidth ikut menghitung padding sebagai bagian dari
// dirinya, padahal children hanya punya ruang di content-box saja.
const measureContent = async () => {
  if (!dashRef.value || !dashWrapperRef.value) return

  const wrapperEl = dashWrapperRef.value
  const wrapperStyle = window.getComputedStyle(wrapperEl)
  const paddingX = parseFloat(wrapperStyle.paddingLeft) + parseFloat(wrapperStyle.paddingRight)

  contentWidth.value = Math.min(wrapperEl.clientWidth - paddingX, 1760)
  await nextTick()

  contentHeight.value = dashRef.value.scrollHeight
}

const calculateScale = async () => {
  await measureContent()
  if (!isAutoFit.value) return

  const availableWidth = window.innerWidth - 80
  const availableHeight = window.innerHeight - 75

  const scaleX = availableWidth / contentWidth.value
  const scaleY = availableHeight / contentHeight.value

  scale.value = Math.min(scaleX, scaleY, 1)
}

const zoomIn = () => {
  isAutoFit.value = false
  scale.value = Math.min(scale.value + 0.1, 2)
  nextTick(() => measureContent())
}

const zoomOut = () => {
  isAutoFit.value = false
  scale.value = Math.max(scale.value - 0.1, 0.3)
  nextTick(() => measureContent())
}

const toggleAutoFit = () => {
  isAutoFit.value = true
  calculateScale()
}

const handleZoomInput = (e) => {
  let val = e.target.value.trim().toLowerCase()
  // Kembali ke Auto jika diketik 'auto' atau dikosongkan
  if (val === 'auto' || val === '') {
    isAutoFit.value = true
    calculateScale()
    return
  }
  
  // Ambil hanya angka
  val = val.replace(/[^0-9]/g, '')
  if (val) {
    let num = parseInt(val, 10)
    // Batasi antara 10% hingga 200%
    if (num < 10) num = 10
    if (num > 200) num = 200
    
    isAutoFit.value = false
    scale.value = num / 100
    nextTick(() => measureContent())
  }

  // Jika input invalid, value akan kembali karena reactivity dari Vue
  // (atau kita set manual untuk memastikan UI terupdate)
  e.target.value = isAutoFit.value ? 'Auto' : Math.round(scale.value * 100) + '%'
}

const captureDashboard = async () => {
  if (!dashRef.value || isCapturing.value) return

  isCapturing.value = true
  try {
    const canvas = await html2canvas(dashRef.value, {
      backgroundColor: '#edf0f7',
      scale: 2,
      useCORS: true,
      // Ambil pada ukuran asli (bukan hasil scale zoom saat ini), supaya hasil
      // capture selalu utuh & konsisten resolusinya berapa pun level zoom-nya.
      onclone: (clonedDoc) => {
        const clonedDash = clonedDoc.querySelector('.dash')
        if (clonedDash) clonedDash.style.transform = 'none'
      },
    })

    const blob = await new Promise((resolve) => canvas.toBlob(resolve, 'image/png'))
    if (!blob) return

    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    const timestamp = new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')
    link.href = url
    link.download = `regional4-sifp-dashboard-${timestamp}.png`
    link.click()
    URL.revokeObjectURL(url)
  } finally {
    isCapturing.value = false
  }
}

// Menghitung ukuran container agar scrollbar asli (native) bekerja saat di-zoom
const scalerStyle = computed(() => {
  if (isAutoFit.value) return { width: '100%', height: '100%' }
  return {
    width: `${contentWidth.value * scale.value}px`,
    height: `${contentHeight.value * scale.value}px`
  }
})

let wrapperResizeObserver = null

onMounted(() => {
  // Matikan scroll global, kita handle scroll di wrapper
  document.body.style.overflow = 'hidden'

  // Hitung skala saat pertama kali dimuat
  calculateScale()

  // Pakai ResizeObserver (bukan cuma window 'resize') karena lebar wrapper
  // juga berubah akibat hal-hal di luar resize browser, misalnya animasi
  // collapse sidebar di App.vue yang baru settle setelah DashboardPage
  // mount duluan — window 'resize' tidak pernah terpicu untuk kasus itu,
  // sehingga contentWidth bisa nyangkut di pengukuran yang terlalu awal/sempit.
  wrapperResizeObserver = new ResizeObserver(() => {
    calculateScale()
  })
  if (dashWrapperRef.value) wrapperResizeObserver.observe(dashWrapperRef.value)
})

onUnmounted(() => {
  // Kembalikan scroll saat pindah halaman
  document.body.style.overflow = ''
  wrapperResizeObserver?.disconnect()
})
</script>

<template>
  <div class="dash-wrapper" ref="dashWrapperRef" :class="{ 'is-scrollable': !isAutoFit }">
    <div class="dash-scaler" :style="scalerStyle">
      <div
        class="dash"
        ref="dashRef"
        :style="{
          width: `${contentWidth}px`,
          transform: `scale(${scale})`,
          transformOrigin: isAutoFit ? 'top center' : 'top left'
        }"
      >
        <DashboardHeader />

    <QuickFacts v-if="visibleCards.quickFacts" class="dash-quickfacts" />

    <div v-if="showKpiRow" class="dash-row dash-row--kpi">
      <GaugeCard v-if="visibleCards.psec" :kpi="kpis[0]" class="flex-1" />
      <GaugeCard v-if="visibleCards.ccvc" :kpi="kpis[1]" class="flex-1" />
      <GaugeCard v-if="visibleCards.psie" :kpi="kpis[2]" class="flex-1" />
      <ConformanceCard v-if="visibleCards.conformance" class="flex-1-4" />
      <ZoneScoresCard v-if="visibleCards.zoneScores" class="flex-2-6" />
    </div>

    <div v-if="showMidRow" class="dash-row dash-row--mid">
      <HealthMap v-if="visibleCards.healthMap" class="dash-healthmap" />

      <div v-if="showTop5Top || showTop5Bottom" class="dash-topfive-block">
        <div v-if="showTop5Top" class="dash-row dash-row--top5-top">
          <TopFivePanel v-if="visibleCards.topPanel1" :panel="topPanels[0]" class="flex-1" />
          <TopFivePanel v-if="visibleCards.topPanel2" :panel="topPanels[1]" class="flex-1-12" />
          <TopFivePanel v-if="visibleCards.topPanel3" :panel="topPanels[2]" class="flex-1-44" />
        </div>
        <div v-if="showTop5Bottom" class="dash-row dash-row--top5-bottom">
          <TopFivePanel v-if="visibleCards.topPanel4" :panel="topPanels[3]" class="flex-1" />
          <TrendChart v-if="visibleCards.trendChart" class="flex-1-6" />
          <ZonaChart v-if="visibleCards.zonaChart" class="flex-0-95" />
        </div>
      </div>
    </div>

    <div v-if="showBottomRow" class="dash-row dash-row--bottom">
      <InitiativesTable v-if="visibleCards.initiatives" class="flex-1" />
      <ExecutiveSummary v-if="visibleCards.executiveSummary" class="flex-1" />
    </div>

    <div class="dash-note">
      <span class="dash-note__icon"><DashIcon name="target" :size="24" /></span>
      <p><strong>Note:</strong> {{ footerNote }}</p>
    </div>
      </div>
    </div>

    <!-- Zoom & Settings Controls Floating -->
    <div class="zoom-controls">
      <button
        class="zoom-btn"
        :disabled="isCapturing"
        @click="captureDashboard"
        :title="isCapturing ? 'Menyiapkan gambar...' : 'Capture Dashboard'"
      >
        <DashIcon :name="isCapturing ? 'refresh' : 'camera'" :size="16" />
      </button>
      <div class="zoom-divider"></div>
      <button class="zoom-btn" @click="showSettings = true" title="Dashboard Settings">
        <DashIcon name="gear" :size="16" />
      </button>
      <div class="zoom-divider"></div>
      <button
        class="zoom-btn"
        :class="{ 'zoom-btn--active': isAutoFit }"
        @click="toggleAutoFit"
        title="Auto Fit to Screen"
      >
        <DashIcon name="auto" :size="16" />
      </button>
      <div class="zoom-divider"></div>
      <button class="zoom-btn" @click="zoomOut" title="Zoom Out">
        <DashIcon name="minus" :size="16" />
      </button>
      
      <input 
        type="text" 
        class="zoom-input" 
        :value="isAutoFit ? 'Auto' : Math.round(scale * 100) + '%'"
        @change="handleZoomInput"
        @focus="$event.target.select()"
        title="Ketik angka (misal: 100) lalu Enter, atau ketik 'Auto'"
      />

      <button class="zoom-btn" @click="zoomIn" title="Zoom In">
        <DashIcon name="plus" :size="16" />
      </button>
    </div>

    <!-- Settings Modal -->
    <div v-if="showSettings" class="settings-backdrop" @click="showSettings = false">
      <div class="settings-modal" @click.stop>
        <div class="settings-header">
          <h3>Customize Dashboard</h3>
          <button @click="showSettings = false" class="close-btn"><DashIcon name="close" :size="20" /></button>
        </div>
        <div class="settings-body">
          <p class="settings-desc">Pilih card yang ingin ditampilkan pada dashboard:</p>
          
          <div class="settings-grid">
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.psec"> PSEC Coverage</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.ccvc"> CCVC Coverage</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.psie"> PSIE Effectiveness</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.conformance"> Conformance Score</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.zoneScores"> Zone Scores</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.quickFacts"> Quick Facts</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.healthMap"> Critical Control Health Map</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.topPanel1"> Top 5 SIF Exposures</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.topPanel2"> Top 5 Safeguard Gaps</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.topPanel3"> Top 5 Recurring Drift</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.topPanel4"> Top 5 Systemic Issues</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.trendChart"> Conformance Trend</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.zonaChart"> Observation by Zone/Month</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.initiatives"> Improvement Initiatives</label>
            <label class="setting-item"><input type="checkbox" v-model="visibleCards.executiveSummary"> Executive Summary</label>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dash-wrapper {
  display: flex;
  /* horizontal: margin auto di child. vertical: safe center (fallback ke start jika overflow, agar tidak terpotong) */
  align-items: safe center;
  height: calc(100vh - 70px);
  width: 100%;
  overflow: hidden;
  padding-top: 1rem;
}

.dash-wrapper.is-scrollable {
  overflow: auto;
}

.dash-scaler {
  transition: width 0.2s, height 0.2s;
  display: flex;
  align-items: flex-start; /* Cegah .dash stretch mengisi full tinggi wrapper */
  margin: 0 auto; /* Trik agar center tapi tidak terpotong saat overflow (safe center) */
}

.dash {
  max-width: 1760px;
  transform-origin: top center;
  transition: transform 0.2s ease-out;
  padding: 0.9rem 1rem 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.dash-row {
  display: flex;
  gap: 0.8rem;
  width: 100%;
}

.flex-1 { flex: 1; }
.flex-1-4 { flex: 1.4; }
.flex-2-6 { flex: 2.6; }
.flex-1-12 { flex: 1.12; }
.flex-1-44 { flex: 1.44; }
.flex-1-6 { flex: 1.6; }
.flex-0-95 { flex: 0.95; }

.dash-quickfacts {
  width: 100%;
}

/* Section 1 (Health Map) tinggi di kiri, menyamai tinggi 2 sub-baris
   Top 5/Trend/Tabel di kanannya (meniru layout slide). */
.dash-row--mid {
  align-items: stretch;
}

.dash-healthmap {
  flex: 0 0 29%;
}

.dash-topfive-block {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
  min-width: 0;
}

.dash-row--top5-top,
.dash-row--top5-bottom {
  flex: 1;
}

.dash-row--bottom {
  align-items: stretch;
}

.dash-note {
  display: flex;
  align-items: flex-start;
  gap: 0.7rem;
  padding: 0 0.4rem;
  color: var(--navy-bar);
}

.dash-note p {
  margin: 0;
  font-size: 0.68rem;
  font-weight: 600;
  line-height: 1.5;
  color: var(--ink);
  max-width: 640px;
}

.dash-note__icon {
  flex: none;
  margin-top: -0.05rem;
}

@media (max-width: 1500px) {
  .dash-row {
    flex-wrap: wrap;
  }
  .flex-1, .flex-1-4, .flex-2-6, .flex-1-12, .flex-1-44, .flex-1-6, .flex-0-95 {
    flex: 1 1 30%;
  }
  .dash-healthmap {
    flex: 1 1 100% !important;
  }
}

@media (max-width: 900px) {
  .flex-1, .flex-1-4, .flex-2-6, .flex-1-12, .flex-1-44, .flex-1-6, .flex-0-95 {
    flex: 1 1 100%;
  }
}

/* Zoom Controls Styles */
.zoom-controls {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  display: flex;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  overflow: hidden;
  z-index: 50;
  border: 1px solid #e2e8f0;
}

.zoom-btn {
  background: transparent;
  border: none;
  padding: 0.6rem 0.8rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #0a1232;
  transition: background 0.2s;
}

.zoom-btn:hover {
  background: #f1f5f9;
}

.zoom-btn--active {
  color: #2563eb;
  background: #eff6ff;
}

.zoom-btn--active:hover {
  background: #dbeafe;
}

.zoom-btn:disabled {
  cursor: default;
  opacity: 0.5;
}

.zoom-btn:disabled:hover {
  background: transparent;
}

.zoom-btn:disabled svg {
  animation: dash-spin 0.8s linear infinite;
}

@keyframes dash-spin {
  to { transform: rotate(360deg); }
}

.zoom-input {
  width: 70px;
  text-align: center;
  font-size: 0.75rem;
  font-weight: 700;
  color: #0a1232;
  background: transparent;
  border: none;
  border-left: 1px solid #e2e8f0;
  border-right: 1px solid #e2e8f0;
  outline: none;
  padding: 0 0.2rem;
}

.zoom-input:focus {
  background: #f8fafc;
}

.zoom-divider {
  width: 1px;
  background: #e2e8f0;
  margin: 0.4rem 0;
}

/* Settings Modal Styles */
.settings-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(10, 18, 50, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.settings-modal {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 500px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.2);
  display: flex;
  flex-direction: column;
}

.settings-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.2rem 1.5rem;
  border-bottom: 1px solid #e2e8f0;
}

.settings-header h3 {
  margin: 0;
  font-size: 1.1rem;
  color: #0a1232;
}

.close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.2rem;
}

.close-btn:hover {
  color: #ef4444;
}

.settings-body {
  padding: 1.5rem;
}

.settings-desc {
  margin: 0 0 1rem 0;
  font-size: 0.9rem;
  color: #64748b;
}

.settings-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.8rem;
}

.setting-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  color: #0a1232;
  cursor: pointer;
  user-select: none;
}

.setting-item input {
  cursor: pointer;
  width: 16px;
  height: 16px;
}
</style>
