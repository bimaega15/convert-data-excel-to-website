<script setup>
import { computed } from 'vue'
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import { api } from '../../services/api'
import { useApiRows } from '../../composables/useApiResource'
import { useDeleteRows } from '../../composables/useDeleteRows'

const { rows, loading, error, reload } = useApiRows((signal) =>
  api.get('/api/observations/all', { signal })
)

// Observasi punya data turunan; endpoint hapus tunggal sudah menghapus berjenjang,
// jadi hapus massal cukup memanggilnya untuk tiap baris terpilih.
const { deleting, deleteError, onDelete } = useDeleteRows(
  (keys) => Promise.all(keys.map((k) => api.delete(`/api/observations/${k}`))),
  reload
)

const columns = [
  { key: 'id', label: 'Obs ID', nowrap: true },
  { key: 'date', label: 'Tanggal', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'protocolName', label: 'Nama Protocol', nowrap: true },
  { key: 'zona', label: 'Zona', align: 'center' },
  { key: 'site', label: 'Site', nowrap: true },
  { key: 'area', label: 'Area / Equipment', clamp: true },
  { key: 'activity', label: 'Aktivitas', clamp: true },
  { key: 'company', label: 'Perusahaan', clamp: true },
  { key: 'observers', label: 'Observer', nowrap: true },
  { key: 'yes', label: 'YES', align: 'center' },
  { key: 'no', label: 'NO', align: 'center' },
  { key: 'na', label: 'NA', align: 'center' },
  { key: 'performance', label: 'Score', align: 'center' },
  { key: 'sequence', label: 'Seq', align: 'center' },
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
        row-key="key"
        :deleting="deleting"
        :error-text="deleteError && deleteError.message"
        @delete="onDelete"
      >
        <template #cell-protocolCode="{ row }">
          <span class="chip" :title="row.protocolName">{{ row.protocolCode }}</span>
        </template>
        <template #cell-observers="{ value }">{{ (value || []).join(', ') || '-' }}</template>
        <template #cell-zona="{ value }">
          <span class="chip">Z{{ value }}</span>
        </template>
        <template #cell-performance="{ value }">
          <span v-if="value != null" class="score-badge" :class="`score-badge--${band(value)}`">
            {{ value.toFixed(1) }}%
          </span>
          <template v-else>-</template>
        </template>
        <template #cell-status="{ value }">
          <span class="pill pill--na">{{ value }}</span>
        </template>
      </DataTable>
    </DataState>
  </div>
</template>
