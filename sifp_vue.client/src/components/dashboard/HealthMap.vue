<script setup>
import { healthMap, statusLegend } from '../../data/dashboard'

const statusLabel = Object.fromEntries(statusLegend.map((l) => [l.status, l.label]))

function regionalClass(status) {
  return status === 'failed' ? 'hm-regional--failed' : 'hm-regional--ok'
}

function cellTitle(row, zoneIdx, cell) {
  const label = statusLabel[cell.status]
  const score = cell.score ?? cell.value
  return `${row.name} — ${healthMap.zones[zoneIdx]}: ${label}${score != null ? ` (${fmt(score)})` : ''}`
}

function fmt(value) {
  return `${value.toFixed(2)}%`
}
</script>

<template>
  <section class="panel" aria-label="Critical Control Health Map by Zona">
    <div class="panel-head panel-head--navy panel-head--numbered">
      <span class="panel-head__num">1</span>
      <span>CRITICAL CONTROL HEALTH MAP</span>
    </div>
    <div class="panel-body hm-body">
      <div class="hm-scroll">
        <table class="hm-table">
          <thead>
            <tr>
              <th class="hm-th-desc">Control</th>
              <th v-for="zone in healthMap.zones" :key="zone">{{ zone }}</th>
              <th>R4 Score</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in healthMap.rows" :key="row.name">
              <td class="hm-name">{{ row.name }}</td>
              <td v-for="(cell, j) in row.cells" :key="j" class="hm-cell">
                <span class="hm-dot">
                  <span
                    class="status-swatch"
                    :class="`status-swatch--${cell.status}`"
                    :title="cellTitle(row, j, cell)"
                  ></span>
                  <span v-if="cell.value != null" class="hm-cell-val">{{ fmt(cell.value) }}</span>
                </span>
              </td>
              <td class="hm-regional" :class="row.regional != null ? regionalClass(row.regionalStatus) : ''">
                {{ row.regional != null ? fmt(row.regional) : '-' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <ul class="hm-legend">
        <li v-for="item in statusLegend" :key="item.status">
          <span class="status-dot" :class="`status-dot--${item.status}`"></span>
          <span>{{ item.label }}</span>
        </li>
      </ul>
    </div>
  </section>
</template>

<style scoped>
.hm-body {
  display: flex;
  flex-direction: column;
  padding: 0.35rem 0.6rem 0.6rem;
}

.hm-scroll {
  overflow-x: auto;
  flex: 1;
}

.hm-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 380px;
}

.hm-table th {
  font-size: 0.66rem;
  font-weight: 800;
  color: var(--navy-bar);
  text-align: center;
  padding: 0.35rem 0.3rem;
  border-bottom: 1px solid var(--line);
  white-space: nowrap;
}

.hm-th-desc {
  text-align: left !important;
  padding-left: 0.3rem !important;
}

.hm-table td {
  padding: 0.28rem 0.3rem;
  border-bottom: 1px solid var(--line-soft);
  font-size: 0.68rem;
}

/* garis pemisah hanya antar kolom zona, bukan antara nomor urut dan nama CLSR */
.hm-cell,
.hm-regional {
  border-left: 1px solid var(--line-soft);
}

.hm-table tbody tr:nth-child(even) {
  background: #f4f7fc;
}

.hm-table tbody tr:last-child td {
  border-bottom: none;
}

.hm-name {
  font-weight: 700;
  color: #222222;
  padding-left: 0.3rem;
  line-height: 1.2;
}

.hm-cell {
  text-align: center;
}

.hm-dot {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
}

.hm-cell-val {
  font-weight: 800;
  color: var(--ink);
}

.hm-regional {
  text-align: center;
  font-weight: 800;
  color: #082c68;
}

.hm-regional--failed {
  color: var(--st-failed);
}

.hm-legend {
  list-style: none;
  margin: 0.3rem 0 0;
  padding: 0.45rem 0.4rem 0;
  border-top: 1px solid var(--line);
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem 1.1rem;
  justify-content: center;
}

.hm-legend li {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.64rem;
  font-weight: 700;
  color: var(--ink);
}
</style>
