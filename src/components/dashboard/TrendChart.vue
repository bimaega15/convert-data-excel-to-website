<script setup>
import { computed, ref } from 'vue'
import { trend } from '../../data/dashboard'

const VB_W = 480
const VB_H = 250
const PAD = { l: 42, r: 36, t: 30, b: 34 }
const plotW = VB_W - PAD.l - PAD.r
const plotH = VB_H - PAD.t - PAD.b

const x = (i) => PAD.l + (i * plotW) / (trend.points.length - 1)
const y = (v) => PAD.t + (1 - v / 100) * plotH

const gridLines = [0, 25, 50, 75, 100]

const linePath = computed(() =>
  trend.points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${x(i)} ${y(p.value)}`).join(' ')
)

const hover = ref(null) // index titik yang di-hover

function onMove(evt) {
  const rect = evt.currentTarget.getBoundingClientRect()
  const vx = ((evt.clientX - rect.left) / rect.width) * VB_W
  let best = 0
  let bestDist = Infinity
  trend.points.forEach((_, i) => {
    const d = Math.abs(vx - x(i))
    if (d < bestDist) {
      bestDist = d
      best = i
    }
  })
  hover.value = best
}

const tipStyle = computed(() => {
  if (hover.value == null) return {}
  return {
    left: `${(x(hover.value) / VB_W) * 100}%`,
    top: `${(y(trend.points[hover.value].value) / VB_H) * 100}%`,
  }
})
</script>

<template>
  <section class="panel" aria-label="Regional Conformance Score Trend">
    <div class="panel-head panel-head--navy">6. REGIONAL CONFORMANCE SCORE TREND (%)</div>
    <div class="panel-body trend-body">
      <div class="chart-wrap" @mousemove="onMove" @mouseleave="hover = null">
        <svg :viewBox="`0 0 ${VB_W} ${VB_H}`" role="img" aria-label="Line chart tren conformance score Jan–May 2026">
          <!-- grid -->
          <g>
            <line
              v-for="v in gridLines"
              :key="v"
              :x1="PAD.l"
              :x2="VB_W - PAD.r"
              :y1="y(v)"
              :y2="y(v)"
              stroke="#edf0f7"
              stroke-width="1"
            />
            <text v-for="v in gridLines" :key="`l${v}`" :x="PAD.l - 7" :y="y(v) + 3" class="axis-label" text-anchor="end">
              {{ v }}%
            </text>
          </g>

          <!-- garis target -->
          <line
            :x1="PAD.l"
            :x2="VB_W - PAD.r"
            :y1="y(trend.target)"
            :y2="y(trend.target)"
            stroke="var(--st-effective)"
            stroke-width="1.6"
            stroke-dasharray="6 4"
          />
          <text :x="VB_W - PAD.r" :y="y(trend.target) - 7" class="target-label" text-anchor="end">
            {{ trend.targetLabel }}
          </text>

          <!-- crosshair -->
          <line
            v-if="hover != null"
            :x1="x(hover)"
            :x2="x(hover)"
            :y1="PAD.t"
            :y2="VB_H - PAD.b"
            stroke="#c3cbe4"
            stroke-width="1"
          />

          <!-- garis data -->
          <path :d="linePath" fill="none" stroke="var(--st-failed)" stroke-width="2.5" stroke-linejoin="round" />

          <!-- titik + label -->
          <g v-for="(p, i) in trend.points" :key="p.month">
            <circle
              :cx="x(i)"
              :cy="y(p.value)"
              :r="hover === i ? 6 : 4.5"
              fill="var(--st-failed)"
              stroke="#fff"
              stroke-width="2"
            />
            <text :x="x(i)" :y="y(p.value) - 11" class="point-label" text-anchor="middle">
              {{ p.value.toFixed(2) }}%
            </text>
            <text :x="x(i)" :y="VB_H - PAD.b + 18" class="axis-label" text-anchor="middle">
              {{ p.month }}
            </text>
          </g>
        </svg>

        <div v-if="hover != null" class="chart-tip" :style="tipStyle">
          {{ trend.points[hover].month }} · {{ trend.points[hover].value.toFixed(2) }}%
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.trend-body {
  display: flex;
  align-items: center;
}

.chart-wrap {
  width: 100%;
}

.chart-wrap svg {
  display: block;
  width: 100%;
  height: auto;
}

.axis-label {
  font-size: 10.5px;
  font-weight: 700;
  fill: var(--ink);
  font-family: inherit;
}

.target-label {
  font-size: 10.5px;
  font-weight: 700;
  fill: var(--st-effective);
  font-family: inherit;
}

.point-label {
  font-size: 11px;
  font-weight: 800;
  fill: var(--ink-strong);
  font-family: inherit;
}
</style>
