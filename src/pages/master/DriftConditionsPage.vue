<script setup>
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import rows from '../../data/generated/drift-conditions.json'

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'code', label: 'Kode', nowrap: true },
  { key: 'level1', label: 'Drift Level 1', nowrap: true },
  { key: 'level2', label: 'Drift Level 2', nowrap: true },
  { key: 'situation', label: 'Situasi Drift', clamp: true },
  { key: 'reason', label: 'Alasan / Penjelasan', clamp: true },
]

const obsCount = new Set(rows.map((r) => r.obsId)).size
const level1Count = new Set(rows.map((r) => r.level1)).size
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Drift Conditions"
      subtitle="Kondisi penyimpangan (drift) dari standar kerja yang terobservasi di lapangan, diklasifikasikan per level."
    >
      <template #right>
        <span class="stat-chip">Total temuan <strong>{{ rows.length }}</strong></span>
        <span class="stat-chip">Observasi <strong>{{ obsCount }}</strong></span>
        <span class="stat-chip">Klasifikasi <strong>{{ level1Count }}</strong></span>
      </template>
    </PageHeader>

    <DataTable :columns="columns" :rows="rows" :initial-sort="{ key: 'obsId', dir: 'asc' }">
      <template #cell-protocolCode="{ value, row }">
        <span class="chip" :title="row.protocolName">{{ value }}</span>
      </template>
      <template #cell-level1="{ value }">
        <span class="pill pill--progress">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
