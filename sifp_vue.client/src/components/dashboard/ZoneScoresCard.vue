<script setup>
import { zonaScores, zoneShort } from '../../data/dashboard'
import RingGauge from './RingGauge.vue'

// Sama seperti threshold status di health map/legend (Effective >=80,
// Degraded 50-<80, Failed <50) — dihitung dari nilai asli, bukan warna
// tetap, supaya tetap benar kalau skor zona berubah.
function colorFor(value) {
  if (value < 50) return '#E10600'
  if (value < 80) return '#F0A400'
  return '#07913B'
}
</script>

<template>
  <section class="panel zone-scores" aria-label="Zone scores">
    <div class="panel-body zone-scores__body">
      <h2 class="zone-scores__title">Zone scores</h2>

      <div class="zone-scores__row">
        <template v-for="(bar, i) in zonaScores.bars" :key="bar.zone">
          <div v-if="i > 0" class="zone-scores__divider"></div>
          <div class="zone-scores__item">
            <span class="zone-scores__label">{{ zoneShort(bar.zone) }}</span>
            <RingGauge :value="bar.value" :color="colorFor(bar.value)" track-color="#D2D8DE" :size="84" :thickness="9">
              <span class="zone-scores__value" :style="{ color: colorFor(bar.value) }">{{ Math.round(bar.value) }}%</span>
            </RingGauge>
          </div>
        </template>
      </div>
    </div>
  </section>
</template>

<style scoped>
.zone-scores {
  background: #f6fbff;
  border-color: #1683d8;
  border-width: 1.5px;
}

.zone-scores__body {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 0.6rem;
}

.zone-scores__title {
  margin: 0 0 0.4rem;
  font-size: 0.95rem;
  font-weight: 800;
  color: #082c68;
  text-align: center;
}

.zone-scores__row {
  display: flex;
  align-items: center;
  justify-content: space-evenly;
  width: 100%;
  flex: 1;
}

.zone-scores__divider {
  width: 1px;
  align-self: stretch;
  margin: 0.3rem 0;
  background: #8799ad;
  opacity: 0.45;
}

.zone-scores__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.2rem;
}

.zone-scores__label {
  font-size: 0.75rem;
  font-weight: 600;
  color: #111111;
}

.zone-scores__value {
  font-size: 1rem;
  font-weight: 800;
}
</style>
