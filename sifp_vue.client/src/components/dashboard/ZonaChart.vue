<script setup>
import { computed } from 'vue'
import { observationsByMonth, zonaScores, zoneShort } from '../../data/dashboard'

const monthTotal = computed(() =>
  observationsByMonth.reduce((sum, m) => sum + (m.count ?? 0), 0)
)

const zoneTotal = computed(() =>
  zonaScores.bars.reduce((sum, b) => sum + (b.obs ?? 0), 0)
)
</script>

<template>
  <section class="panel" aria-label="Observation by Zone and Month">
    <div class="panel-head panel-head--navy panel-head--numbered">
      <span class="panel-head__num">7</span>
      <span>OBSERVATION BY ZONE / MONTH</span>
    </div>
    <div class="panel-body obm-body">
      <table class="obm-table">
        <thead>
          <tr>
            <th class="obm-th-label">Month</th>
            <th v-for="m in observationsByMonth" :key="m.month">{{ m.month }}</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="obm-th-label">Obs.</td>
            <td v-for="m in observationsByMonth" :key="m.month">{{ m.count }}</td>
            <td class="obm-total">{{ monthTotal }}</td>
          </tr>
        </tbody>
      </table>

      <div class="obm-sub">Observations by Zone (Total {{ zoneTotal }})</div>

      <table class="obm-table">
        <thead>
          <tr>
            <th v-for="bar in zonaScores.bars" :key="bar.zone">{{ zoneShort(bar.zone) }}</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td v-for="bar in zonaScores.bars" :key="bar.zone">{{ bar.obs }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style scoped>
.obm-body {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.6rem 0.8rem;
}

.obm-table {
  width: 100%;
  border-collapse: collapse;
}

.obm-table th {
  font-size: 0.78rem;
  font-weight: 800;
  color: var(--navy-bar);
  text-align: center;
  padding: 0.3rem 0.2rem;
  border-bottom: 1px solid var(--line);
}

.obm-table td {
  font-size: 0.78rem;
  font-weight: 600;
  color: #111111;
  text-align: center;
  padding: 0.35rem 0.2rem;
}

.obm-th-label {
  text-align: left !important;
}

.obm-total {
  font-weight: 800;
  color: var(--navy-bar);
}

.obm-sub {
  font-size: 0.66rem;
  font-weight: 800;
  color: var(--ink);
  text-align: center;
  padding-top: 0.2rem;
  border-top: 1px solid var(--line-soft);
}
</style>
