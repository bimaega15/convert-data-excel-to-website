<script setup>
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import rows from '../../data/generated/ccvc-library.json'

const columns = [
  { key: 'no', label: 'No', align: 'center' },
  { key: 'psecId', label: 'PSEC ID', nowrap: true },
  { key: 'psecName', label: 'SIF Exposure (PSEC)', nowrap: true },
  { key: 'ccvcId', label: 'CCVC ID', nowrap: true },
  { key: 'questionCode', label: 'Kode', align: 'center' },
  { key: 'questionSummary', label: 'Ringkasan Pertanyaan', clamp: true },
  { key: 'verificationPurpose', label: 'Tujuan Verifikasi', clamp: true },
  { key: 'protocolGroup', label: 'Grup', nowrap: true },
]

const psecCount = new Set(rows.map((r) => r.psecId)).size
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

    <DataTable :columns="columns" :rows="rows" :initial-sort="{ key: 'no', dir: 'asc' }">
      <template #cell-psecId="{ value }">
        <span class="chip">{{ value }}</span>
      </template>
      <template #cell-protocolGroup="{ value }">
        <span class="pill pill--na">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
