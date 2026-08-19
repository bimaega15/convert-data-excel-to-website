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
  fetchAllPages('/api/master/error-traps', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => api.post('/api/master/error-traps/delete', { ids: keys }),
  reload
)

const showModal = ref(false)
const submitting = ref(false)
const submitError = ref('')

async function handleAdd(formData) {
  submitting.value = true
  submitError.value = ''
  try {
    await api.post('/api/master/error-traps/create', formData)
    showModal.value = false
    reload()
  } catch (err) {
    submitError.value = err.message || 'Gagal menambah error trap.'
  } finally {
    submitting.value = false
  }
}

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'protocolName', label: 'Nama Protocol', nowrap: true },
  { key: 'category', label: 'Kategori', nowrap: true },
  { key: 'errorTrap', label: 'Error Trap', nowrap: true },
  { key: 'comments', label: 'Penjelasan', clamp: true },
]

const obsCount = computed(() => new Set(rows.value.map((r) => r.obsId)).size)
const categoryCount = computed(() => new Set(rows.value.map((r) => r.category)).size)
const pretty = (s) => (s ? s.replaceAll('_', ' ') : '-')
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Error Traps"
      subtitle="Jebakan kesalahan (human performance) yang teridentifikasi pada setiap observasi, dikelompokkan per kategori."
    >
      <template #right>
        <span class="stat-chip">Total temuan <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">Observasi <strong>{{ obsCount }}</strong></span>
        <span class="stat-chip">Kategori <strong>{{ categoryCount }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'obsId', dir: 'asc' }"
        selectable
        can-add
        add-label="Tambah Error Trap"
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @add="showModal = true"
        @delete="onDelete"
      >
        <template #cell-protocolCode="{ value, row }">
          <span class="chip" :title="row.protocolName">{{ value }}</span>
        </template>
        <template #cell-category="{ value }">
          <span class="chip chip--accent">{{ pretty(value) }}</span>
        </template>
        <template #cell-errorTrap="{ value }">
          <strong>{{ pretty(value) }}</strong>
        </template>
      </DataTable>
    </DataState>

    <AddRowModal
      :show="showModal"
      title="Tambah Error Trap Baru"
      :columns="columns"
      :submitting="submitting"
      :error-text="submitError"
      @close="showModal = false"
      @submit="handleAdd"
    />
  </div>
</template>
