<script setup>
// Panel status bersama untuk halaman yang memuat data dari API:
// loading, gagal (dengan tombol coba lagi), atau kosong.
import DashIcon from '../dashboard/DashIcon.vue'

defineProps({
  loading: { type: Boolean, default: false },
  error: { type: Object, default: null },
  empty: { type: Boolean, default: false },
  emptyText: { type: String, default: 'Belum ada data. Import workbook terlebih dahulu.' },
  loadingText: { type: String, default: 'Memuat data dari server…' },
})

const emit = defineEmits(['retry'])
</script>

<template>
  <div v-if="loading" class="panel data-state">
    <span class="data-state__spinner" aria-hidden="true"></span>
    <p>{{ loadingText }}</p>
  </div>

  <div v-else-if="error" class="panel data-state data-state--error">
    <DashIcon name="warning" :size="22" />
    <div class="data-state__body">
      <p class="data-state__title">Gagal memuat data</p>
      <p class="data-state__detail">{{ error.message }}</p>
      <p v-if="error.status" class="data-state__hint">
        HTTP {{ error.status }} · pastikan backend berjalan di
        <code>Sifp_Vue.Server</code> (<code>dotnet run</code>).
      </p>
    </div>
    <button type="button" class="data-state__retry" @click="emit('retry')">Coba lagi</button>
  </div>

  <div v-else-if="empty" class="panel data-state">
    <DashIcon name="file" :size="20" />
    <p>{{ emptyText }}</p>
  </div>

  <slot v-else />
</template>

<style scoped>
.data-state {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  padding: 1.5rem 1.1rem;
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.data-state p {
  margin: 0;
}

.data-state--error {
  align-items: flex-start;
  border-left: 3px solid var(--accent-red, #d93025);
}

.data-state__body {
  flex: 1;
  min-width: 0;
}

.data-state__title {
  font-weight: 800;
  color: var(--ink-strong);
}

.data-state__detail {
  margin-top: 0.2rem !important;
  color: var(--ink);
}

.data-state__hint {
  margin-top: 0.3rem !important;
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.data-state__hint code {
  background: #f1f3fa;
  border-radius: 5px;
  padding: 0.05rem 0.3rem;
  font-size: 0.68rem;
}

.data-state__retry {
  flex: none;
  border: 1px solid var(--line);
  border-radius: 9px;
  background: #fff;
  padding: 0.35rem 0.8rem;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--ink);
  font-family: inherit;
}

.data-state__retry:hover {
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

.data-state__spinner {
  flex: none;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid var(--line);
  border-top-color: var(--accent-blue, #1d40b0);
  animation: data-state-spin 0.7s linear infinite;
}

@keyframes data-state-spin {
  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .data-state__spinner {
    animation-duration: 2s;
  }
}
</style>
