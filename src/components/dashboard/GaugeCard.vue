<script setup>
import { computed } from 'vue'

const props = defineProps({
  kpi: { type: Object, required: true },
})

const colors = {
  green: 'var(--accent-green)',
  blue: 'var(--accent-blue)',
  purple: 'var(--accent-purple)',
}

const color = computed(() => colors[props.kpi.variant] ?? 'var(--accent-green)')

// setengah lingkaran r=80: panjang busur = PI * r
const ARC_LEN = Math.PI * 80
const dash = computed(() => `${(props.kpi.value / 100) * ARC_LEN} ${ARC_LEN}`)

const valueLabel = computed(() => `${props.kpi.value.toFixed(1)}%`)
</script>

<template>
  <section class="panel gauge-card" :aria-label="`${kpi.code} ${valueLabel}`">
    <div class="panel-body gauge-card__body">
      <h2 class="gauge-card__code" :style="{ color }">{{ kpi.code }}</h2>
      <p class="gauge-card__title">{{ kpi.title }}</p>
      <p class="gauge-card__value" :style="{ color }">{{ valueLabel }}</p>
      <p class="gauge-card__desc">{{ kpi.desc }}</p>

      <div class="gauge-card__gauge">
        <svg viewBox="0 0 200 112" role="img" :aria-label="`Gauge ${valueLabel}`">
          <path
            d="M 20 100 A 80 80 0 0 1 180 100"
            fill="none"
            stroke="var(--track)"
            stroke-width="17"
            stroke-linecap="round"
          />
          <path
            d="M 20 100 A 80 80 0 0 1 180 100"
            fill="none"
            :stroke="color"
            stroke-width="17"
            stroke-linecap="round"
            :stroke-dasharray="dash"
          />
          <text x="100" y="93" class="gauge-card__center">{{ valueLabel }}</text>
        </svg>
        <div class="gauge-card__scale">
          <span>0%</span>
          <span>100%</span>
        </div>
      </div>
    </div>

    <div class="target-pill">
      <span>{{ kpi.target }}</span>
      <span class="info-badge">i</span>
    </div>
  </section>
</template>

<style scoped>
.gauge-card__body {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding-bottom: 0;
}

.gauge-card__code {
  margin: 0.1rem 0 0;
  font-size: 1.5rem;
  font-weight: 800;
}

.gauge-card__title {
  margin: 0;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--ink);
  min-height: 2em;
}

.gauge-card__value {
  margin: 0.3rem 0 0;
  font-size: 1.7rem;
  font-weight: 800;
  line-height: 1;
}

.gauge-card__desc {
  margin: 0.25rem 0 0.4rem;
  font-size: 0.66rem;
  font-weight: 600;
  color: var(--ink-muted);
  min-height: 2.1em;
}

.gauge-card__gauge {
  width: min(180px, 90%);
}

.gauge-card__gauge svg {
  display: block;
  width: 100%;
  height: auto;
}

.gauge-card__center {
  font-size: 22px;
  font-weight: 800;
  fill: var(--ink-strong);
  text-anchor: middle;
  font-family: inherit;
}

.gauge-card__scale {
  display: flex;
  justify-content: space-between;
  font-size: 0.62rem;
  font-weight: 700;
  color: var(--ink);
  margin-top: -0.35rem;
  padding: 0 0.15rem;
}
</style>
