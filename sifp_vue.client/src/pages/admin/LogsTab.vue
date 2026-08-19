<script setup>
import { ref, onMounted } from 'vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import { fetchAllPages } from '../../services/api'

const logs = ref([])
const loading = ref(true)
const error = ref('')

const columns = [
  { key: 'id', label: 'Log ID', align: 'center' },
  { key: 'timestamp', label: 'Waktu (UTC)', nowrap: true },
  { key: 'username', label: 'User / Actor', nowrap: true },
  { key: 'module', label: 'Modul', align: 'center' },
  { key: 'action', label: 'Aktivitas / Event', nowrap: true },
  { key: 'details', label: 'Rincian Maintenance / Action', clamp: true },
  { key: 'ipAddress', label: 'IP Address', nowrap: true },
  { key: 'statusCode', label: 'Status Code', align: 'center' },
]

async function loadLogs() {
  loading.value = true
  error.value = ''
  try {
    const res = await fetchAllPages('/api/logs')
    logs.value = res || []
  } catch (err) {
    error.value = err.message || 'Gagal memuat log sistem.'
  } finally {
    loading.value = false
  }
}

onMounted(loadLogs)
</script>

<template>
  <div class="admin-tab">
    <div class="tab-header">
      <h3 class="tab-title">Log System for Maintenance & Audit</h3>
      <p class="tab-subtitle">Pencatatan riwayat aktivitas pengguna, perubahan data master, otentikasi, dan maintenance sistem.</p>
    </div>

    <DataState :loading="loading" :error="error" :empty="!logs.length" @retry="loadLogs">
      <DataTable
        :columns="columns"
        :rows="logs"
        :initial-sort="{ key: 'timestamp', dir: 'desc' }"
      >
        <template #cell-module="{ value }">
          <span class="chip chip--accent">{{ value || 'SYSTEM' }}</span>
        </template>
        <template #cell-action="{ value }">
          <strong>{{ value }}</strong>
        </template>
        <template #cell-statusCode="{ value }">
          <span :class="['pill', value < 400 ? 'pill--yes' : 'pill--no']">{{ value || 200 }}</span>
        </template>
      </DataTable>
    </DataState>
  </div>
</template>

<style scoped>
.admin-tab { display: flex; flex-direction: column; gap: 1rem; }
.tab-title { font-size: 1.1rem; font-weight: 800; color: var(--ink); margin: 0; }
.tab-subtitle { font-size: 0.76rem; color: var(--ink-muted); margin: 0.2rem 0 0; }
</style>
