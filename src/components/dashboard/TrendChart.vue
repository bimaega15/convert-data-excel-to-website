<script setup>
import { computed, ref } from 'vue'
import { trend } from '../../data/dashboard'

const VB_W = 480
const VB_H = 250
const PAD = { l: 42, r: 36, t: 30, b: 34 }
const plotW = VB_W - PAD.l - PAD.r
const plotH = VB_H - PAD.t - PAD.b

// gabungan titik aktual + proyeksi pada satu sumbu bulan
const actual = trend.points
const projection = trend.projection ?? []
const allPoints = [
  ...actual.map((p) => ({ ...p, projected: false })),
  ...projection.map((p) => ({ ...p, projected: true })),
]

const x = (i) => PAD.l + (i * plotW) / Math.max(allPoints.length - 1, 1)
const y = (v) => PAD.t + (1 - v / 100) * plotH

const gridLines = [0, 25, 50, 75, 100]

const actualPath = computed(() =>
  actual.map((p, i) => `${i === 0 ? 'M' : 'L'} ${x(i)} ${y(p.value)}`).join(' ')
)

// proyeksi menyambung dari titik aktual terakhir
const projectionPath = computed(() => {
  if (!projection.length || !actual.length) return ''
  const startIdx = actual.length - 1
  const parts = [`M ${x(startIdx)} ${y(actual[startIdx].value)}`]
  projection.forEach((p, i) => parts.push(`L ${x(startIdx + 1 + i)} ${y(p.value)}`))
  return parts.join(' ')
})

const hover = ref(null)

function onMove(evt) {
  const rect = evt.currentTarget.getBoundingClientRect()
  const vx = ((evt.clientX - rect.left) / rect.width) * VB_W
  let best = 0
  let bestDist = Infinity
  allPoints.forEach((_, i) => {
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
    top: `${(y(allPoints[hover.value].value) / VB_H) * 100}%`,
  }
})

// label angka: semua titik aktual + titik proyeksi terakhir
function showLabel(i) {
  return !allPoints[i].projected || i === allPoints.length - 1
}
</script>

<template>
  <section class="panel" aria-label="Regional Conformance Score Trend">
    <div class="panel-head panel-head--navy">6. REGIONAL CONFORMANCE SCORE TREND (%)</div>
    <div class="panel-body trend-body">
      <div class="chart-wrap" @mousemove="onMove" @mouseleave="hover = null">
        <svg :viewBox="`0 0 ${VB_W} ${VB_H}`" role="img" aria-label="Line chart tren conformance score aktual dan proyeksi">
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
          <text :x="PAD.l + 4" :y="y(trend.target) - 7" class="target-label" text-anchor="start">
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

          <!-- garis proyeksi (putus-putus) lalu garis aktual di atasnya -->
          <path
            v-if="projectionPath"
            :d="projectionPath"
            fill="none"
            stroke="var(--ink-muted)"
            stroke-width="2"
            stroke-dasharray="5 5"
            stroke-linejoin="round"
          />
          <path :d="actualPath" fill="none" stroke="var(--st-failed)" stroke-width="2.5" stroke-linejoin="round" />

          <!-- titik + label -->
          <g v-for="(p, i) in allPoints" :key="p.month">
            <circle
              :cx="x(i)"
              :cy="y(p.value)"
              :r="hover === i ? 6 : 4.5"
              :fill="p.projected ? '#fff' : 'var(--st-failed)'"
              :stroke="p.projected ? 'var(--ink-muted)' : '#fff'"
              stroke-width="2"
            />
            <text v-if="showLabel(i)" :x="x(i)" :y="y(p.value) - 11" class="point-label" text-anchor="middle">
              {{ p.value.toFixed(2) }}%
            </text>
            <text :x="x(i)" :y="VB_H - PAD.b + 18" class="axis-label" text-anchor="middle">
              {{ p.month }}
            </text>
          </g>
        </svg>

        <div v-if="hover != null" class="chart-tip" :style="tipStyle">
          {{ allPoints[hover].month }} · {{ allPoints[hover].value.toFixed(2) }}%
          <template v-if="allPoints[hover].projected">(proyeksi)</template>
        </div>
      </div>

      <div class="trend-legend">
        <span><i class="trend-legend__line trend-legend__line--actual"></i> Aktual</span>
        <span v-if="projection.length"><i class="trend-legend__line trend-legend__line--proj"></i> Proyeksi</span>
      </div>
    </div>
  </section>
</template>

<style scoped>
.trend-body {
  display: flex;
  flex-direction: column;
  justify-content: center;
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

.trend-legend {
  display: flex;
  justify-content: center;
  gap: 1.2rem;
  font-size: 0.64rem;
  font-weight: 700;
  color: var(--ink);
  padding-top: 0.15rem;
}

.trend-legend span {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
}

.trend-legend__line {
  display: inline-block;
  width: 22px;
  height: 0;
  border-top: 3px solid var(--st-failed);
  border-radius: 2px;
}

.trend-legend__line--proj {
  border-top-style: dashed;
  border-top-color: var(--ink-muted);
  border-top-width: 2px;
}
</style>
