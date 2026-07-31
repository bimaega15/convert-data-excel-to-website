<script setup>
import { computed } from 'vue'
import PageHeader from '../../components/ui/PageHeader.vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import { fetchAllPages } from '../../services/api'
import { useApiRows } from '../../composables/useApiResource'

const { rows, loading, error, reload } = useApiRows((signal) =>
  fetchAllPages('/api/master/sif-questions', { signal })
)

const columns = [
  { key: 'obsId', label: 'Obs ID', nowrap: true },
  { key: 'ccvcId', label: 'CCVC ID', nowrap: true },
  { key: 'question', label: 'Pertanyaan Observasi', clamp: true },
  { key: 'answer', label: 'Jawaban', align: 'center' },
  { key: 'criticalSafeguard', label: 'Critical Safeguard', nowrap: true },
  { key: 'sifExposure', label: 'SIF Exposure', nowrap: true },
  { key: 'comments', label: 'Komentar', clamp: true },
  { key: 'zona', label: 'Zona', align: 'center' },
  { key: 'site', label: 'Site', nowrap: true },
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
      <DataTable :columns="columns" :rows="rows" :initial-sort="{ key: 'obsId', dir: 'asc' }">
        <template #cell-ccvcId="{ value, row }">
          <span class="chip" :title="row.protocolName">{{ value }}</span>
        </template>
        <template #cell-answer="{ value }">
          <span class="pill" :class="answerPill[value] ?? 'pill--na'">{{ value }}</span>
        </template>
        <template #cell-zona="{ value }">
          <span class="chip">Z{{ value }}</span>
        </template>
      </DataTable>
    </DataState>
  </div>
</template>
