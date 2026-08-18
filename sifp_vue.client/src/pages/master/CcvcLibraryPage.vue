<script setup>
import { computed } from 'vue'
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import { fetchAllPages, api } from '../../services/api'
import { useApiRows } from '../../composables/useApiResource'
import { useDeleteRows } from '../../composables/useDeleteRows'

const { rows, loading, error, reload } = useApiRows((signal) =>
  fetchAllPages('/api/master/ccvc-library', { signal })
)

const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => api.post('/api/master/ccvc-library/delete', { ids: keys }),
  reload
)

const columns = [
  { key: 'no', label: 'No', align: 'center' },
  { key: 'psecId', label: 'PSEC ID', nowrap: true },
  { key: 'psecName', label: 'SIF Exposure (PSEC)', nowrap: true },
  { key: 'exposureType', label: 'Tipe Exposure', nowrap: true },
  { key: 'ccvcId', label: 'CCVC ID', nowrap: true },
  { key: 'questionCode', label: 'Kode', align: 'center' },
  { key: 'questionSummary', label: 'Ringkasan Pertanyaan', clamp: true },
  { key: 'verificationPurpose', label: 'Tujuan Verifikasi', clamp: true },
  { key: 'protocolGroup', label: 'Grup', nowrap: true },
]

const psecCount = computed(() => new Set(rows.value.map((r) => r.psecId)).size)
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="PSEC & CCVC Library"
      subtitle="Master library denominator: daftar SIF Exposure (PSEC) dan critical safeguard/pertanyaan verifikasi (CCVC) resmi."
    >
      <template #right>
        <span class="stat-chip">Total CCVC <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">PSEC / CLSR <strong>{{ psecCount }}</strong></span>
      </template>
    </PageHeader>

    <DataState :loading="loading" :error="error" :empty="!rows.length" @retry="reload">
      <DataTable
        :columns="columns"
        :rows="rows"
        :initial-sort="{ key: 'no', dir: 'asc' }"
        selectable
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @delete="onDelete"
      >
        <template #cell-psecId="{ value }">
          <span class="chip">{{ value }}</span>
        </template>
        <template #cell-protocolGroup="{ value }">
          <span class="pill pill--na">{{ value }}</span>
        </template>
      </DataTable>
    </DataState>
  </div>
</template>
