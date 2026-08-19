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
  api.get('/api/initiatives/all', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => Promise.all(keys.map((k) => api.delete(`/api/initiatives/${k}`))),
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
      improvementCode: formData.id || `IMP-R4-${Date.now().toString().slice(-3)}`,
      initiative: formData.initiative,
      relatedClsr: formData.relatedClsr,
      owner: formData.owner,
      status: formData.status || 'In Progress',
      progressPercent: Number(formData.progress || 0),
      expectedImpact: formData.expectedImpact,
      notes: formData.notes
    }
    await api.post('/api/initiatives', payload)
    showModal.value = false
    reload()
  } catch (err) {
    submitError.value = err.message || 'Gagal menambah inisiatif.'
  } finally {
    submitting.value = false
  }
}

const columns = [
  { key: 'id', label: 'ID', nowrap: true },
  { key: 'initiative', label: 'Inisiatif', clamp: true },
  { key: 'relatedClsr', label: 'CLSR Terkait', nowrap: true },
  { key: 'owner', label: 'Owner', nowrap: true },
  { key: 'status', label: 'Status', align: 'center' },
  { key: 'progress', label: 'Progress', width: '180px', type: 'number' },
  { key: 'expectedImpact', label: 'Dampak yang Diharapkan', clamp: true },
  { key: 'notes', label: 'Catatan', nowrap: true },
]

const inProgress = computed(() => rows.value.filter((r) => r.status === 'In Progress').length)
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Improvement Initiatives"
      subtitle="Daftar inisiatif perbaikan hasil observasi V&V beserta owner, status, dan progres implementasinya."
    >
      <template #right>
        <span class="stat-chip">Total inisiatif <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">In Progress <strong>{{ inProgress }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'id', dir: 'asc' }"
        selectable
        can-add
        add-label="Tambah Inisiatif"
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @add="showModal = true"
        @delete="onDelete"
      >
        <template #cell-relatedClsr="{ value }">
          <span class="chip">{{ value }}</span>
        </template>
        <template #cell-status="{ value }">
          <span class="pill pill--progress">{{ value }}</span>
        </template>
        <template #cell-progress="{ value }">
          <div class="progress-cell">
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: `${value ?? 0}%` }"></div>
            </div>
            <span class="progress-val">{{ value ?? 0 }}%</span>
          </div>
        </template>
      </DataTable>
    </DataState>

    <AddRowModal
      :show="showModal"
      title="Tambah Inisiatif Baru"
      :columns="columns"
      :submitting="submitting"
      :error-text="submitError"
      @close="showModal = false"
      @submit="handleAdd"
    />
  </div>
</template>

<style scoped>
.progress-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.progress-track {
  flex: 1;
  height: 8px;
  background: var(--track);
  border-radius: 999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--st-effective);
  border-radius: 999px;
}

.progress-val {
  flex: none;
  width: 2.4rem;
  text-align: right;
  font-size: 0.66rem;
  font-weight: 800;
  color: var(--ink);
}
</style>
