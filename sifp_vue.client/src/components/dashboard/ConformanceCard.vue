<script setup>
import { computed } from 'vue'
import { conformance } from '../../data/dashboard'

const statusColor = {
  failed: 'var(--st-failed)',
  degraded: 'var(--st-degraded)',
  effective: 'var(--st-effective)',
}

const valueLabel = computed(() => `${conformance.value.toFixed(2)}%`)
</script>

<template>
  <section class="panel conf-card" :aria-label="`Regional 4 Conformance Score ${valueLabel}`">
    <div class="panel-body conf-card__body">
      <h2 class="conf-card__title">REGIONAL 4<br />CONFORMANCE SCORE</h2>
      <p class="conf-card__sub">(Overall Average)</p>
      <p class="conf-card__value">{{ valueLabel }}</p>

      <div class="conf-card__meter">
        <div class="conf-card__bar">
          <div
            v-for="band in conformance.bands"
            :key="band.status"
            class="conf-card__seg"
            :style="{ width: `${band.to - band.from}%`, background: statusColor[band.status] }"
            :title="`${band.from}–${band.to}%`"
          ></div>
          <div class="conf-card__marker" :style="{ left: `${conformance.value}%` }"></div>
        </div>
        <div class="conf-card__scale">
          <span>0%</span>
          <span>100%</span>
        </div>
      </div>
    </div>

    <div class="target-pill">
      <span>{{ conformance.target }}</span>
      <span class="info-badge">i</span>
    </div>
  </section>
</template>

<style scoped>
.conf-card__body {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding-bottom: 0;
}

.conf-card__title {
  margin: 0.1rem 0 0;
  font-size: 0.95rem;
  font-weight: 800;
  line-height: 1.2;
  color: var(--accent-red);
}

.conf-card__sub {
  margin: 0.15rem 0 0;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink);
}

.conf-card__value {
  margin: 1rem 0 1.1rem;
  font-size: 2.6rem;
  font-weight: 800;
  line-height: 1;
  color: var(--accent-red);
}

.conf-card__meter {
  width: 100%;
  padding: 0 0.25rem;
}

.conf-card__bar {
  position: relative;
  display: flex;
  gap: 2px; /* spacer antar segmen (spec dataviz) */
  height: 13px;
  border-radius: 999px;
  overflow: visible;
}

.conf-card__seg {
  height: 100%;
}

.conf-card__seg:first-child {
  border-radius: 999px 0 0 999px;
}

.conf-card__seg:last-child {
  border-radius: 0 999px 999px 0;
}

.conf-card__marker {
  position: absolute;
  top: 50%;
  width: 12px;
  height: 12px;
  background: #111;
  border: 2px solid #fff;
  transform: translate(-50%, -50%) rotate(45deg);
  box-shadow: 0 0 3px rgba(0, 0, 0, 0.4);
}

.conf-card__scale {
  display: flex;
  justify-content: space-between;
  font-size: 0.62rem;
  font-weight: 700;
  color: var(--ink);
  margin-top: 0.35rem;
}
</style>
