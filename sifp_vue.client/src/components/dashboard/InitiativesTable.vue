<script setup>
import { initiatives } from '../../data/dashboard'
</script>

<template>
  <section class="panel" aria-label="Systemic Improvement Initiatives">
    <div class="panel-head panel-head--navy panel-head--numbered">
      <span class="panel-head__num">8</span>
      <span>SYSTEMIC IMPROVEMENT INITIATIVES</span>
    </div>
    <div class="panel-body init-body">
      <div class="init-scroll">
        <table class="init-table">
          <thead>
            <tr>
              <th class="col-num">No.</th>
              <th class="col-name">Initiative</th>
              <th>Owner</th>
              <th>Status</th>
              <th class="col-progress">Progress</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, i) in initiatives" :key="item.name">
              <td class="col-num">{{ i + 1 }}</td>
              <td class="init-name">{{ item.name }}</td>
              <td class="init-owner">{{ item.owner }}</td>
              <td class="init-status">
                <span class="status-dot-inline"></span>
                <span>{{ item.status }}</span>
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
  min-width: 480px;
}

.init-table th {
  font-size: 0.68rem;
  font-weight: 700;
  color: #464646;
  text-align: left;
  padding: 0.4rem 0.45rem;
  border-bottom: 1px solid var(--line);
  white-space: nowrap;
}

.init-table th:nth-child(3),
.init-table th:nth-child(4) {
  text-align: center;
}

.init-table td {
  padding: 0.4rem 0.45rem;
  border-bottom: 1px solid var(--line-soft);
  font-size: 0.7rem;
  color: #282828;
  vertical-align: middle;
}

.init-table tbody tr:last-child td {
  border-bottom: none;
}

.col-num {
  width: 1.6rem;
  font-weight: 600;
}

.col-progress {
  width: 34%;
}

.init-name {
  font-weight: 600;
  line-height: 1.3;
}

.init-owner {
  text-align: center;
  white-space: nowrap;
}

.init-status {
  text-align: center;
  white-space: nowrap;
}

.status-dot-inline {
  display: inline-block;
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: #f49820;
  margin-right: 0.35rem;
}

.progress-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.progress-track {
  flex: 1;
  height: 8px;
  background: #e0e4eb;
  border-radius: 999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: #2962a4;
  border-radius: 999px;
}

.progress-val {
  flex: none;
  width: 2.2rem;
  text-align: right;
  font-size: 0.68rem;
  font-weight: 700;
  color: #282828;
}
</style>
