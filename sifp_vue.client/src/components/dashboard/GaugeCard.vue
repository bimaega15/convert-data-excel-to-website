<script setup>
import { computed } from 'vue'
import RingGauge from './RingGauge.vue'

const props = defineProps({
  kpi: { type: Object, required: true },
})

// Palet per KPI mengikuti desain slide (bukan `kpi.variant` dari backend,
// yang cuma label semantik lama "green/blue/purple" — CCVC misalnya kini
// tampil oranye di desain baru, bukan biru).
const styles = {
  PSEC: { bg: '#F7FFF7', border: '#078A37', ring: '#078A37', text: '#111111' },
  CCVC: { bg: '#FFFDF5', border: '#F0A400', ring: '#F0A400', text: '#111111' },
  PSIE: { bg: '#F7F5FF', border: '#6D28D9', ring: '#6D28D9', text: '#3F1678' },
}

const style = computed(() => styles[props.kpi.code] ?? styles.PSEC)

const valueLabel = computed(() => (props.kpi.pending ? 'Pending' : `${props.kpi.value.toFixed(props.kpi.value % 1 === 0 ? 0 : 2)}%`))
</script>

<template>
  <section
    class="panel gauge-card"
    :style="{ background: style.bg, borderColor: style.border }"
    :aria-label="`${kpi.code} ${valueLabel}`"
  >
    <div class="panel-body gauge-card__body">
      <h2 class="gauge-card__title" :style="{ color: style.text }">{{ kpi.code }}</h2>

      <RingGauge
        :value="kpi.pending ? 0 : kpi.value"
        :color="kpi.pending ? '#D2D8DE' : style.ring"
        track-color="#D2D8DE"
        :size="132"
        :thickness="13"
        class="gauge-card__ring"
      >
        <span class="gauge-card__value" :style="{ color: kpi.pending ? 'var(--ink-muted)' : style.text }">
          {{ valueLabel }}
        </span>
      </RingGauge>

      <p class="gauge-card__desc" :style="{ color: style.text }">{{ kpi.desc }}</p>
    </div>
  </section>
</template>

<style scoped>
.gauge-card {
  border-width: 1.5px;
}

.gauge-card__body {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 0.3rem;
}

.gauge-card__title {
  margin: 0;
  font-size: 1rem;
  font-weight: 800;
}

.gauge-card__ring {
  margin: 0.15rem 0;
}

.gauge-card__value {
  font-size: 1.35rem;
  font-weight: 800;
  line-height: 1;
}

.gauge-card__desc {
  margin: 0;
  font-size: 0.72rem;
  font-weight: 700;
  line-height: 1.3;
}
</style>
