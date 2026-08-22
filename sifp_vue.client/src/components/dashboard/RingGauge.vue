<script setup>
import { computed } from 'vue'

const props = defineProps({
  value: { type: Number, required: true },
  color: { type: String, required: true },
  size: { type: Number, default: 140 },
  thickness: { type: Number, default: 14 },
  trackColor: { type: String, default: 'var(--track)' },
  // Celah di bagian bawah lingkaran (derajat), memberi kesan "gauge" bukan donut penuh.
  gapDeg: { type: Number, default: 52 },
})

const r = computed(() => props.size / 2 - props.thickness / 2)
const c = computed(() => Math.PI * 2 * r.value)
const sweepDeg = computed(() => 360 - props.gapDeg)
const sweepLen = computed(() => (sweepDeg.value / 360) * c.value)
const startDeg = computed(() => 90 + props.gapDeg / 2)

const clamped = computed(() => Math.min(Math.max(props.value, 0), 100))
const valueLen = computed(() => (clamped.value / 100) * sweepLen.value)

const trackDash = computed(() => `${sweepLen.value} ${c.value - sweepLen.value}`)
const valueDash = computed(() => `${valueLen.value} ${c.value - valueLen.value}`)
</script>

<template>
  <div class="ring-gauge" :style="{ width: `${size}px`, height: `${size}px` }">
    <svg :viewBox="`0 0 ${size} ${size}`" role="img" :aria-label="`${value}%`">
      <circle
        :cx="size / 2"
        :cy="size / 2"
        :r="r"
        fill="none"
        :stroke="trackColor"
        :stroke-width="thickness"
        stroke-linecap="round"
        :stroke-dasharray="trackDash"
        :transform="`rotate(${startDeg} ${size / 2} ${size / 2})`"
      />
      <circle
        v-if="clamped > 0"
        :cx="size / 2"
        :cy="size / 2"
        :r="r"
        fill="none"
        :stroke="color"
        :stroke-width="thickness"
        stroke-linecap="round"
        :stroke-dasharray="valueDash"
        :transform="`rotate(${startDeg} ${size / 2} ${size / 2})`"
      />
    </svg>
    <div class="ring-gauge__center">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.ring-gauge {
  position: relative;
  flex: none;
}

.ring-gauge svg {
  display: block;
  width: 100%;
  height: 100%;
}

.ring-gauge__center {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
}
</style>
