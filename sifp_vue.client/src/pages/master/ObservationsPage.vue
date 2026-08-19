<script setup>
import { computed, ref } from 'vue'
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import AddRowModal from '../../components/ui/AddRowModal.vue'
import { api } from '../../services/api'
import { useApiRows } from '../../composables/useApiResource'
import { useDeleteRows } from '../../composables/useDeleteRows'

const { rows, loading, error, reload } = useApiRows((signal) =>
  api.get('/api/observations/all', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => Promise.all(keys.map((k) => api.delete(`/api/observations/${k}`))),
  reload
)

const showModal = ref(false)
const submitting = ref(false)
const submitError = ref('')

async function handleAdd(formData) {
  submitting.value = true
  submitError.value = ''
  try {
    const payload = {
      obsCode: formData.id || `OBS-${Date.now().toString().slice(-4)}`,
      protocolCode: formData.protocolCode,
      protocolName: formData.protocolName,
      observationDate: formData.date || new Date().toISOString().split('T')[0],
      zona: Number(formData.zona || 1),
      site: formData.site,
      areaEquipment: formData.area,
      activity: formData.activity,
      company: formData.company,
      observer1: formData.observers,
      yesCount: Number(formData.yes || 0),
      noCount: Number(formData.no || 0),
      naCount: Number(formData.na || 0),
      performancePercent: Number(formData.performance || 100),
      status: formData.status || 'Active',
      isActive: true
    }
    await api.post('/api/observations', payload)
    showModal.value = false
    reload()
  } catch (err) {
    submitError.value = err.message || 'Gagal menambah observasi.'
  } finally {
    submitting.value = false
  }
}

const columns = [
  { key: 'id', label: 'Obs ID', nowrap: true },
  { key: 'date', label: 'Tanggal', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'protocolName', label: 'Nama Protocol', nowrap: true },
  { key: 'zona', label: 'Zona', align: 'center', type: 'number' },
  { key: 'site', label: 'Site', nowrap: true },
  { key: 'area', label: 'Area / Equipment', clamp: true },
  { key: 'activity', label: 'Aktivitas', clamp: true },
  { key: 'company', label: 'Perusahaan', clamp: true },
  { key: 'observers', label: 'Observer', nowrap: true },
  { key: 'yes', label: 'YES', align: 'center', type: 'number' },
  { key: 'no', label: 'NO', align: 'center', type: 'number' },
  { key: 'na', label: 'NA', align: 'center', type: 'number' },
  { key: 'performance', label: 'Score', align: 'center', type: 'number' },
  { key: 'sequence', label: 'Seq', align: 'center', type: 'number' },
  { key: 'psieEligible', label: 'PSIE', align: 'center' },
  { key: 'status', label: 'Status', align: 'center' },
  { key: 'active', label: 'Aktif', align: 'center' },
]

const band = (v) => (v < 50 ? 'failed' : v < 80 ? 'degraded' : 'effective')
const sites = computed(() => new Set(rows.value.map((r) => r.site)).size)
const zones = computed(() => new Set(rows.value.map((r) => r.zona)).size)
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Observations"
      subtitle="Rekap observasi V&V per Obs ID — hasil verifikasi lapangan (YES/NO/NA) beserta skor performance."
    >
      <template #right>
        <span class="stat-chip">Total observasi <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">Site <strong>{{ sites }}</strong></span>
        <span class="stat-chip">Zona <strong>{{ zones }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'id', dir: 'asc' }"
        selectable
        can-add
        add-label="Tambah Observasi"
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @add="showModal = true"
        @delete="onDelete"
      >
        <template #cell-protocolCode="{ row }">
          <span class="chip" :title="row.protocolName">{{ row.protocolCode }}</span>
        </template>
        <template #cell-observers="{ value }">{{ Array.isArray(value) ? value.join(', ') : (value || '-') }}</template>
        <template #cell-zona="{ value }">
          <span class="chip">Z{{ value }}</span>
        </template>
        <template #cell-performance="{ value }">
          <span v-if="value != null" class="score-badge" :class="`score-badge--${band(value)}`">
            {{ Number(value).toFixed(1) }}%
          </span>
          <template v-else>-</template>
        </template>
        <template #cell-status="{ value }">
          <span class="pill pill--na">{{ value }}</span>
        </template>
      </DataTable>
    </DataState>

    <AddRowModal
      :show="showModal"
      title="Tambah Observasi Baru"
      :columns="columns"
      :submitting="submitting"
      :error-text="submitError"
      @close="showModal = false"
      @submit="handleAdd"
    />
  </div>
</template>
