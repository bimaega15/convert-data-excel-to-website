<script setup>
import { computed } from 'vue'
import { conformance } from '../../data/dashboard'
import RingGauge from './RingGauge.vue'

const valueLabel = computed(() => `${conformance.value.toFixed(2)}%`)
</script>

<template>
  <section class="panel conf-card" :aria-label="`Regional 4 Conformance Score ${valueLabel}`">
    <div class="panel-body conf-card__body">
      <h2 class="conf-card__title">Regional 4 Conformance Score</h2>

      <RingGauge :value="conformance.value" color="#E10600" track-color="#D2D8DE" :size="132" :thickness="13">
        <span class="conf-card__value">{{ valueLabel }}</span>
      </RingGauge>
    </div>
  </section>
</template>

<style scoped>
.conf-card {
  background: #f6fbff;
  border-color: #e10600;
  border-width: 1.5px;
}

.conf-card__body {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 0.3rem;
}

.conf-card__title {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 800;
  line-height: 1.25;
  color: #082c68;
}

.conf-card__value {
  font-size: 1.35rem;
  font-weight: 800;
  line-height: 1;
  color: #e10600;
}
</style>
