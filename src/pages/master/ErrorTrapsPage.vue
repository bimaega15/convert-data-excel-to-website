<script setup>
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import rows from '../../data/generated/error-traps.json'

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'protocolCode', label: 'Protocol', nowrap: true },
  { key: 'category', label: 'Kategori', nowrap: true },
  { key: 'errorTrap', label: 'Error Trap', nowrap: true },
  { key: 'comments', label: 'Penjelasan', clamp: true },
]

const obsCount = new Set(rows.map((r) => r.obsId)).size
const categoryCount = new Set(rows.map((r) => r.category)).size
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

    <DataTable :columns="columns" :rows="rows" :initial-sort="{ key: 'obsId', dir: 'asc' }">
      <template #cell-protocolCode="{ value, row }">
        <span class="chip" :title="row.protocolName">{{ value }}</span>
      </template>
      <template #cell-category="{ value }">
        <span class="pill pill--na">{{ pretty(value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
