<script setup>
import { nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import DashIcon from '../dashboard/DashIcon.vue'
import { meta } from '../../data/dashboard'
import { sheetGroups } from '../../data/sheets'

const props = defineProps({
  open: { type: Boolean, default: false },
  collapsed: { type: Boolean, default: false },
})

const emit = defineEmits(['close'])

// Tooltip mode ikon dipasang di luar area scroll (position: fixed) supaya tidak
// terpotong oleh overflow .sidebar__nav.
const tip = ref({ show: false, text: '', top: 0 })

function showTip(evt, label) {
  if (!props.collapsed) return
  const r = evt.currentTarget.getBoundingClientRect()
  tip.value = { show: true, text: label, top: r.top + r.height / 2 }
}

function hideTip() {
  tip.value.show = false
}

// Fade halus di tepi area scroll: hanya muncul kalau memang masih ada menu di
// atas/bawah, supaya batas daftar tidak terpotong tajam.
const navEl = ref(null)
const atTop = ref(true)
const atBottom = ref(true)

function updateEdges() {
  const el = navEl.value
  if (!el) return
  const max = el.scrollHeight - el.clientHeight
  atTop.value = el.scrollTop <= 2
  atBottom.value = max <= 2 || el.scrollTop >= max - 2
}

let resizeObserver

onMounted(() => {
  updateEdges()
  if (typeof ResizeObserver !== 'undefined') {
    resizeObserver = new ResizeObserver(updateEdges)
    resizeObserver.observe(navEl.value)
  }
  window.addEventListener('resize', updateEdges)
})

onBeforeUnmount(() => {
  resizeObserver?.disconnect()
  window.removeEventListener('resize', updateEdges)
})

// lebar berubah saat mode ikon → tinggi konten ikut berubah
watch(
  () => props.collapsed,
  () => nextTick(updateEdges),
)

// Grup worksheet dibangun otomatis dari manifest hasil konversi Excel, sehingga
// jumlah & isi menu selalu mengikuti worksheet yang ada di file Excel.
const worksheetGroups = sheetGroups.map((group) => ({
  label: group.label,
  items: group.items.map((it) => ({
    to: it.route,
    label: it.label,
    icon: it.icon,
    title: it.name, // nama sheet asli untuk tooltip
    count: it.dataRows,
  })),
}))

const menu = [
  {
    label: 'Menu',
    items: [
      { to: '/', label: 'Dashboard', icon: 'home' },
      { to: '/import', label: 'Import Excel', icon: 'upload' },
    ],
  },
  ...worksheetGroups,
]
</script>

<template>
  <aside class="sidebar" :class="{ 'sidebar--open': open, 'sidebar--collapsed': collapsed }">
    <div class="sidebar__brand">
      <span class="sidebar__mark">R4</span>
      <span class="sidebar__brand-text">
        <strong>SIFP Assurance</strong>
        <small>Regional 4 · Pertamina EP Cepu</small>
      </span>
      <button type="button" class="sidebar__close d-lg-none" aria-label="Tutup menu" @click="emit('close')">
        <DashIcon name="close" :size="18" />
      </button>
    </div>

    <nav
      ref="navEl"
      class="sidebar__nav"
      :class="{ 'sidebar__nav--fade-top': !atTop, 'sidebar__nav--fade-bottom': !atBottom }"
      @scroll.passive="updateEdges"
    >
      <div v-for="group in menu" :key="group.label" class="sidebar__group">
        <p class="sidebar__group-label">{{ group.label }}</p>
        <router-link
          v-for="item in group.items"
          :key="item.to"
          :to="item.to"
          class="sidebar__link"
          :aria-label="item.label"
          :title="item.title ?? item.label"
          @click="emit('close')"
          @mouseenter="showTip($event, item.title ?? item.label)"
          @focus="showTip($event, item.title ?? item.label)"
          @mouseleave="hideTip"
          @blur="hideTip"
        >
          <span class="sidebar__link-icon"><DashIcon :name="item.icon" :size="17" /></span>
          <span class="sidebar__link-text">{{ item.label }}</span>
          <span v-if="item.count != null" class="sidebar__link-count">{{ item.count }}</span>
        </router-link>
      </div>
    </nav>

    <span v-if="collapsed && tip.show" class="sidebar__tooltip" :style="{ top: `${tip.top}px` }">
      {{ tip.text }}
    </span>

    <div class="sidebar__footer">
      <p class="sidebar__source" :title="meta.sourceFile">{{ meta.sourceFile }}</p>
      <p class="sidebar__version">Konversi: {{ meta.generatedAt?.slice(0, 10) }}</p>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  position: fixed;
  inset: 0 auto 0 0;
  width: 264px;
  display: flex;
  flex-direction: column;
  background: linear-gradient(180deg, #14246b 0%, #1e2f83 60%, #24338f 100%);
  color: #fff;
  z-index: 1040;
  transition: width 0.22s ease;
}

.sidebar__brand {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 1.1rem 1.1rem 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.12);
}

.sidebar__mark {
  flex: none;
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: linear-gradient(135deg, #d93025, #f0b429);
  font-weight: 800;
  font-size: 0.95rem;
  color: #fff;
}

.sidebar__brand-text {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
  min-width: 0;
}

.sidebar__brand-text strong {
  font-size: 0.92rem;
  font-weight: 800;
}

.sidebar__brand-text small {
  font-size: 0.62rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.65);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar__close {
  margin-left: auto;
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.8);
  padding: 0.2rem;
}

.sidebar__nav {
  --fade-top: 0px;
  --fade-bottom: 0px;
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.9rem 0.55rem 0.9rem 0.75rem;
  overscroll-behavior: contain;
  scroll-behavior: smooth;
  /* daftar memudar di tepi hanya kalau masih ada menu di arah tersebut */
  -webkit-mask-image: linear-gradient(
    to bottom,
    transparent 0,
    #000 var(--fade-top),
    #000 calc(100% - var(--fade-bottom)),
    transparent 100%
  );
  mask-image: linear-gradient(
    to bottom,
    transparent 0,
    #000 var(--fade-top),
    #000 calc(100% - var(--fade-bottom)),
    transparent 100%
  );
  /* Firefox */
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 255, 255, 0.22) transparent;
}

.sidebar__nav--fade-top {
  --fade-top: 26px;
}

.sidebar__nav--fade-bottom {
  --fade-bottom: 26px;
}

/* scrollbar ramping ala overlay, tanpa tombol panah bawaan Windows */
.sidebar__nav::-webkit-scrollbar {
  width: 6px;
}

.sidebar__nav::-webkit-scrollbar-button {
  display: none;
  height: 0;
  width: 0;
}

.sidebar__nav::-webkit-scrollbar-track {
  background: transparent;
  margin: 4px 0;
}

.sidebar__nav::-webkit-scrollbar-thumb {
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.16);
  transition: background 0.2s ease;
}

.sidebar:hover .sidebar__nav::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.32);
}

.sidebar__nav::-webkit-scrollbar-thumb:hover,
.sidebar__nav::-webkit-scrollbar-thumb:active {
  background: rgba(255, 255, 255, 0.55);
}

.sidebar:hover .sidebar__nav {
  scrollbar-color: rgba(255, 255, 255, 0.32) transparent;
}

@media (prefers-reduced-motion: reduce) {
  .sidebar__nav {
    scroll-behavior: auto;
  }
}

.sidebar__group + .sidebar__group {
  margin-top: 1.1rem;
}

.sidebar__group-label {
  margin: 0 0 0.35rem;
  padding: 0 0.6rem;
  font-size: 0.6rem;
  font-weight: 800;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.45);
  white-space: nowrap;
}

.sidebar__link {
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.52rem 0.6rem;
  margin-bottom: 2px;
  border-radius: 10px;
  color: rgba(255, 255, 255, 0.82);
  text-decoration: none;
  font-size: 0.78rem;
  font-weight: 600;
  transition: background 0.15s, color 0.15s;
}

.sidebar__link:hover {
  background: rgba(255, 255, 255, 0.08);
  color: #fff;
}

.sidebar__link.router-link-exact-active {
  background: rgba(255, 255, 255, 0.16);
  color: #fff;
  font-weight: 700;
}

.sidebar__link-icon {
  flex: none;
  display: inline-flex;
}

.sidebar__link-text {
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar__link-count {
  flex: none;
  min-width: 1.4rem;
  text-align: center;
  padding: 0.02rem 0.32rem;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.12);
  font-size: 0.58rem;
  font-weight: 800;
  color: rgba(255, 255, 255, 0.72);
  font-variant-numeric: tabular-nums;
}

.sidebar__link.router-link-exact-active .sidebar__link-count {
  background: rgba(255, 255, 255, 0.24);
  color: #fff;
}

/* tooltip mode ikon: fixed agar tidak terpotong area scroll menu */
.sidebar__tooltip {
  position: fixed;
  left: 86px;
  transform: translateY(-50%);
  background: var(--ink-strong);
  color: #fff;
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.32rem 0.6rem;
  border-radius: 8px;
  white-space: nowrap;
  box-shadow: 0 4px 14px rgba(10, 18, 50, 0.3);
  pointer-events: none;
  z-index: 1045;
}

.sidebar__footer {
  padding: 0.85rem 1.1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.12);
}

.sidebar__source {
  margin: 0;
  font-size: 0.6rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.6);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar__version {
  margin: 0.15rem 0 0;
  font-size: 0.58rem;
  color: rgba(255, 255, 255, 0.4);
}

/* ===== mode ikon (collapsed) ===== */
@media (min-width: 992px) {
  .sidebar--collapsed {
    width: 76px;
  }

  .sidebar--collapsed .sidebar__brand {
    justify-content: center;
    padding-left: 0;
    padding-right: 0;
  }

  .sidebar--collapsed .sidebar__brand-text,
  .sidebar--collapsed .sidebar__group-label,
  .sidebar--collapsed .sidebar__link-text,
  .sidebar--collapsed .sidebar__link-count,
  .sidebar--collapsed .sidebar__footer {
    display: none;
  }

  .sidebar--collapsed .sidebar__nav {
    padding-left: 0.6rem;
    padding-right: 0.45rem;
  }

  .sidebar--collapsed .sidebar__nav::-webkit-scrollbar {
    width: 4px;
  }

  .sidebar--collapsed .sidebar__group + .sidebar__group {
    margin-top: 0.6rem;
    padding-top: 0.6rem;
    border-top: 1px solid rgba(255, 255, 255, 0.12);
  }

  .sidebar--collapsed .sidebar__link {
    justify-content: center;
    padding: 0.6rem 0;
  }
}

/* layar sentuh tidak punya hover, jadi thumb dibuat lebih terbaca sejak awal */
@media (hover: none) {
  .sidebar__nav {
    scrollbar-color: rgba(255, 255, 255, 0.3) transparent;
  }

  .sidebar__nav::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.3);
  }
}

@media (max-width: 991.98px) {
  .sidebar {
    transform: translateX(-100%);
    transition: transform 0.25s ease;
    box-shadow: none;
  }

  .sidebar--open {
    transform: translateX(0);
    box-shadow: 0 0 40px rgba(0, 0, 0, 0.35);
  }
}
</style>
