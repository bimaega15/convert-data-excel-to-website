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
  fetchAllPages('/api/master/hp-tools', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => api.post('/api/master/hp-tools/delete', { ids: keys }),
  reload
)

const showModal = ref(false)
const submitting = ref(false)
const submitError = ref('')

async function handleAdd(formData) {
  submitting.value = true
  submitError.value = ''
  try {
    await api.post('/api/master/hp-tools/create', formData)
    showModal.value = false
    reload()
  } catch (err) {
    submitError.value = err.message || 'Gagal menambah HP tool baru.'
  } finally {
    submitting.value = false
  }
}

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'protocolName', label: 'Nama Protocol', nowrap: true },
  { key: 'tool', label: 'HP Tool', nowrap: true },
  { key: 'tujuan', label: 'Tujuan', clamp: true },
  { key: 'kapanDigunakan', label: 'Kapan Digunakan', clamp: true },
  { key: 'caraPakai', label: 'Cara Pakai / Implementasi', clamp: true },
  { key: 'effectivenessNotes', label: 'Catatan Efektivitas', clamp: true },
]

const obsCount = computed(() => new Set(rows.value.map((r) => r.obsId)).size)
const toolCount = computed(() => new Set(rows.value.map((r) => r.tool)).size)
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="HP Tools"
      subtitle="Human Performance Tools yang digunakan pada tiap observasi beserta tujuan, cara pakai, dan catatan efektivitasnya."
    >
      <template #right>
        <span class="stat-chip">Total baris <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">Observasi <strong>{{ obsCount }}</strong></span>
        <span class="stat-chip">Jenis tool <strong>{{ toolCount }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'obsId', dir: 'asc' }"
        selectable
        can-add
        add-label="Tambah HP Tool"
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @add="showModal = true"
        @delete="onDelete"
      >
        <template #cell-protocolCode="{ value, row }">
          <span class="chip" :title="row.protocolName">{{ value }}</span>
        </template>
        <template #cell-tool="{ value }">
          <span class="chip chip--accent">{{ value }}</span>
        </template>
      </DataTable>
    </DataState>

    <AddRowModal
      :show="showModal"
      title="Tambah HP Tool Baru"
      :columns="columns"
      :submitting="submitting"
      :error-text="submitError"
      @close="showModal = false"
      @submit="handleAdd"
    />
  </div>
</template>
