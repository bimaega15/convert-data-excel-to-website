<script setup>
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import rows from '../../data/generated/observations.json'

const columns = [
  { key: 'id', label: 'Obs ID', nowrap: true },
  { key: 'date', label: 'Tanggal', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'zona', label: 'Zona', align: 'center' },
  { key: 'site', label: 'Site', nowrap: true },
  { key: 'activity', label: 'Aktivitas', clamp: true },
  { key: 'company', label: 'Perusahaan', clamp: true },
  { key: 'yes', label: 'YES', align: 'center' },
  { key: 'no', label: 'NO', align: 'center' },
  { key: 'na', label: 'NA', align: 'center' },
  { key: 'performance', label: 'Score', align: 'center' },
  { key: 'status', label: 'Status', align: 'center' },
]

const band = (v) => (v < 50 ? 'failed' : v < 80 ? 'degraded' : 'effective')
const sites = new Set(rows.map((r) => r.site)).size
const zones = new Set(rows.map((r) => r.zona)).size
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

    <DataTable :columns="columns" :rows="rows" :initial-sort="{ key: 'id', dir: 'asc' }">
      <template #cell-protocolCode="{ row }">
        <span class="chip" :title="row.protocolName">{{ row.protocolCode }}</span>
      </template>
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
  </div>
</template>
