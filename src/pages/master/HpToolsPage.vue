<script setup>
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import rows from '../../data/generated/hp-tools.json'

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'tool', label: 'HP Tool', nowrap: true },
  { key: 'tujuan', label: 'Tujuan', clamp: true },
  { key: 'kapanDigunakan', label: 'Kapan Digunakan', clamp: true },
  { key: 'caraPakai', label: 'Cara Pakai / Implementasi', clamp: true },
  { key: 'effectivenessNotes', label: 'Catatan Efektivitas', clamp: true },
]

const obsCount = new Set(rows.map((r) => r.obsId)).size
const toolCount = new Set(rows.map((r) => r.tool)).size
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

    <DataTable :columns="columns" :rows="rows" :initial-sort="{ key: 'obsId', dir: 'asc' }">
      <template #cell-protocolCode="{ value, row }">
        <span class="chip" :title="row.protocolName">{{ value }}</span>
      </template>
      <template #cell-tool="{ value }">
        <span class="pill pill--yes">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
