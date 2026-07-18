<script setup>
import { initiatives } from '../../data/dashboard'
</script>

<template>
  <section class="panel" aria-label="Improvement Initiatives Status">
    <div class="panel-head panel-head--navy">8. IMPROVEMENT INITIATIVES (Status)</div>
    <div class="panel-body init-body">
      <div class="init-scroll">
        <table class="init-table">
          <thead>
            <tr>
              <th class="col-num">#</th>
              <th class="col-name">INITIATIVES</th>
              <th>OWNER</th>
              <th>STATUS</th>
              <th class="col-progress">PROGRESS</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, i) in initiatives" :key="item.name">
              <td class="col-num">{{ i + 1 }}</td>
              <td class="init-name">{{ item.name }}</td>
              <td class="init-owner">{{ item.owner }}</td>
              <td class="init-status">
                <span class="status-pill">{{ item.status }}</span>
              </td>
              <td>
                <div class="progress-cell">
                  <div
                    class="progress-track"
                    role="progressbar"
                    :aria-valuenow="item.progress"
                    aria-valuemin="0"
                    aria-valuemax="100"
                    :aria-label="`${item.name}: ${item.progress}%`"
                  >
                    <div class="progress-fill" :style="{ width: `${item.progress}%` }"></div>
                  </div>
                  <span class="progress-val">{{ item.progress }}%</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>

<style scoped>
.init-body {
  padding: 0.4rem 0.7rem 0.6rem;
}

.init-scroll {
  overflow-x: auto;
}

.init-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 560px;
}

.init-table th {
  font-size: 0.64rem;
  font-weight: 800;
  color: var(--ink);
  text-align: left;
  padding: 0.35rem 0.45rem;
  border-bottom: 1px solid var(--line);
  white-space: nowrap;
}

.init-table th:nth-child(3),
.init-table th:nth-child(4) {
  text-align: center;
}

.init-table td {
  padding: 0.34rem 0.45rem;
  border-bottom: 1px solid #eef1f8;
  font-size: 0.68rem;
  vertical-align: middle;
}

.init-table tbody tr:last-child td {
  border-bottom: none;
}

.col-num {
  width: 1.5rem;
  font-weight: 800;
  color: var(--ink);
}

.col-progress {
  width: 32%;
}

.init-name {
  font-weight: 700;
  color: var(--ink);
  line-height: 1.3;
}

.init-owner {
  font-weight: 700;
  color: var(--ink);
  text-align: center;
  white-space: nowrap;
}

.init-status {
  text-align: center;
}

.status-pill {
  display: inline-block;
  background: var(--amber-bright);
  color: var(--ink-strong);
  font-size: 0.62rem;
  font-weight: 800;
  padding: 0.16rem 0.6rem;
  border-radius: 6px;
  white-space: nowrap;
}

.progress-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.progress-track {
  flex: 1;
  height: 8px;
  background: var(--track);
  border-radius: 999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--st-effective);
  border-radius: 999px;
}

.progress-val {
  flex: none;
  width: 2.2rem;
  text-align: right;
  font-size: 0.68rem;
  font-weight: 800;
  color: var(--ink);
}
</style>
