<script setup>
import { computed, ref } from 'vue'
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import AddRowModal from '../../components/ui/AddRowModal.vue'
import { fetchAllPages, api } from '../../services/api'
import { useApiRows } from '../../composables/useApiResource'
import { useDeleteRows } from '../../composables/useDeleteRows'

const { rows, loading, error, reload } = useApiRows((signal) =>
  fetchAllPages('/api/master/latent-conditions', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => api.post('/api/master/latent-conditions/delete', { ids: keys }),
  reload
)

const showModal = ref(false)
const submitting = ref(false)
const submitError = ref('')

async function handleAdd(formData) {
  submitting.value = true
  submitError.value = ''
  try {
    await api.post('/api/master/latent-conditions/create', formData)
    showModal.value = false
    reload()
  } catch (err) {
    submitError.value = err.message || 'Gagal menambah latent condition baru.'
  } finally {
    submitting.value = false
  }
}

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'protocolName', label: 'Nama Protocol', nowrap: true },
  { key: 'code', label: 'Kode', nowrap: true },
  { key: 'level1', label: 'Latent Level 1', nowrap: true },
  { key: 'level2', label: 'Latent Level 2', nowrap: true },
  { key: 'observation', label: 'Latent Observation', clamp: true },
  { key: 'reason', label: 'Alasan / Penjelasan', clamp: true },
  { key: 'sequence', label: 'Seq', align: 'center', type: 'number' },
  { key: 'status', label: 'Status', nowrap: true },
  { key: 'active', label: 'Aktif', align: 'center' },
]

const obsCount = computed(() => new Set(rows.value.map((r) => r.obsId)).size)
const level1Count = computed(() => new Set(rows.value.map((r) => r.level1)).size)
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Latent Conditions"
      subtitle="Kondisi laten organisasi (systemic weakness) yang teridentifikasi dari observasi — akar masalah di balik drift."
    >
      <template #right>
        <span class="stat-chip">Total temuan <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">Observasi <strong>{{ obsCount }}</strong></span>
        <span class="stat-chip">Klasifikasi <strong>{{ level1Count }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'obsId', dir: 'asc' }"
        selectable
        can-add
        add-label="Tambah Latent Condition"
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @add="showModal = true"
        @delete="onDelete"
      >
        <template #cell-protocolCode="{ value, row }">
          <span class="chip" :title="row.protocolName">{{ value }}</span>
        </template>
        <template #cell-level1="{ value }">
          <span class="pill pill--progress">{{ value }}</span>
        </template>
        <template #cell-code="{ value }">
          <span class="chip chip--accent">{{ value }}</span>
        </template>
      </DataTable>
    </DataState>

    <AddRowModal
      :show="showModal"
      title="Tambah Latent Condition Baru"
      :columns="columns"
      :submitting="submitting"
      :error-text="submitError"
      @close="showModal = false"
      @submit="handleAdd"
    />
  </div>
</template>
