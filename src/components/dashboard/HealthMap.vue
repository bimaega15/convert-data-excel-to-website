<script setup>
import { healthMap, statusLegend } from '../../data/dashboard'

const statusLabel = Object.fromEntries(statusLegend.map((l) => [l.status, l.label]))

function regionalClass(value) {
  return value < 50 ? 'hm-regional--failed' : 'hm-regional--ok'
}

function fmt(value) {
  return `${value.toFixed(2)}%`
}
</script>

<template>
  <section class="panel" aria-label="CLSR Exposure Health Map by Zona">
    <div class="panel-head panel-head--navy">1. CLSR EXPOSURE HEALTH MAP (10 CLSR) BY ZONA</div>
    <div class="panel-body hm-body">
      <div class="hm-scroll">
        <table class="hm-table">
          <thead>
            <tr>
              <th class="hm-th-desc" colspan="2">CLSR DESCRIPTION</th>
              <th v-for="zone in healthMap.zones" :key="zone">{{ zone }}</th>
              <th>REGIONAL 4</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, i) in healthMap.rows" :key="row.name">
              <td class="hm-num">{{ i + 1 }}</td>
              <td class="hm-name">{{ row.name }}</td>
              <td v-for="(cell, j) in row.cells" :key="j" class="hm-cell">
                <span class="hm-dot">
                  <span
                    class="status-dot"
                    :class="`status-dot--${cell.status}`"
                    :title="`${row.name} — ${healthMap.zones[j]}: ${statusLabel[cell.status]}${cell.value != null ? ` (${fmt(cell.value)})` : ''}`"
                  ></span>
                  <span v-if="cell.value != null" class="hm-cell-val">{{ fmt(cell.value) }}</span>
                </span>
              </td>
              <td class="hm-regional" :class="row.regional != null ? regionalClass(row.regional) : ''">
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
  min-width: 480px;
}

.hm-table th {
  font-size: 0.66rem;
  font-weight: 800;
  color: var(--ink);
  text-align: center;
  padding: 0.35rem 0.3rem;
  border-bottom: 1px solid var(--line);
  white-space: nowrap;
}

.hm-th-desc {
  text-align: left !important;
  padding-left: 1.6rem !important;
}

.hm-table td {
  padding: 0.28rem 0.3rem;
  border-bottom: 1px solid #eef1f8;
  font-size: 0.68rem;
}

.hm-table tbody tr:nth-child(even) {
  background: #f8fafd;
}

.hm-table tbody tr:last-child td {
  border-bottom: none;
}

.hm-num {
  width: 1.4rem;
  font-weight: 800;
  color: var(--ink);
  text-align: center;
}

.hm-name {
  font-weight: 700;
  color: var(--ink);
  text-transform: uppercase;
  white-space: nowrap;
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
  color: var(--ink-muted);
}

.hm-regional--ok {
  color: var(--ink);
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
