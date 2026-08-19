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
  fetchAllPages('/api/master/sif-questions', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => api.post('/api/master/sif-questions/delete', { ids: keys }),
  reload
)

const showModal = ref(false)
const submitting = ref(false)
const submitError = ref('')

async function handleAdd(formData) {
  submitting.value = true
  submitError.value = ''
  try {
    await api.post('/api/master/sif-questions/create', formData)
    showModal.value = false
    reload()
  } catch (err) {
    submitError.value = err.message || 'Gagal menambah SIF question baru.'
  } finally {
    submitting.value = false
  }
}

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'protocolName', label: 'Nama Protocol', nowrap: true },
  { key: 'questionRef', label: 'Ref', nowrap: true },
  { key: 'ccvcId', label: 'CCVC ID', nowrap: true },
  { key: 'question', label: 'Pertanyaan Observasi', clamp: true },
  { key: 'answer', label: 'Jawaban', align: 'center' },
  { key: 'criticalSafeguard', label: 'Critical Safeguard', nowrap: true },
  { key: 'sifExposure', label: 'SIF Exposure', nowrap: true },
  { key: 'comments', label: 'Komentar', clamp: true },
  { key: 'date', label: 'Tanggal', nowrap: true },
  { key: 'zona', label: 'Zona', align: 'center', type: 'number' },
  { key: 'site', label: 'Site', nowrap: true },
  { key: 'activity', label: 'Aktivitas', clamp: true },
  { key: 'company', label: 'Perusahaan', clamp: true },
]

const answerPill = { YES: 'pill--yes', NO: 'pill--no', NA: 'pill--na' }
const obsCount = computed(() => new Set(rows.value.map((r) => r.obsId)).size)
const noCount = computed(() => rows.value.filter((r) => r.answer === 'NO').length)
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="SIF Questions"
      subtitle="Detail jawaban pertanyaan verifikasi SIF per observasi — satu baris adalah satu critical safeguard yang diverifikasi."
    >
      <template #right>
        <span class="stat-chip">Total baris <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">Observasi <strong>{{ obsCount }}</strong></span>
        <span class="stat-chip">Temuan NO <strong>{{ noCount }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'obsId', dir: 'asc' }"
        selectable
        can-add
        add-label="Tambah SIF Question"
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @add="showModal = true"
        @delete="onDelete"
      >
        <template #cell-protocolCode="{ row }">
          <span class="chip" :title="row.protocolName">{{ row.protocolCode }}</span>
        </template>
        <template #cell-ccvcId="{ value }">
          <span class="chip">{{ value }}</span>
        </template>
        <template #cell-answer="{ value }">
          <span class="pill" :class="answerPill[value] || 'pill--na'">{{ value }}</span>
        </template>
        <template #cell-zona="{ value }">
          <span class="chip">Z{{ value }}</span>
        </template>
      </DataTable>
    </DataState>

    <AddRowModal
      :show="showModal"
      title="Tambah SIF Question Baru"
      :columns="columns"
      :submitting="submitting"
      :error-text="submitError"
      @close="showModal = false"
      @submit="handleAdd"
    />
  </div>
</template>
