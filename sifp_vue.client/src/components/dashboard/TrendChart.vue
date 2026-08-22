<script setup>
import { computed, ref } from 'vue'
import { trend } from '../../data/dashboard'

const VB_W = 480
const VB_H = 250
const PAD = { l: 42, r: 36, t: 44, b: 34 }
const plotW = VB_W - PAD.l - PAD.r
const plotH = VB_H - PAD.t - PAD.b

const ACTUAL_COLOR = '#0B3B78'
const PLAN_COLOR = '#169447'

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

// "Actual — through Jul   Plan / Target – Aug–Dec", dihitung dari data asli
// (bukan teks tetap) supaya selalu cocok dengan bulan yang benar-benar ada.
const captionText = computed(() => {
  const lastActual = actual[actual.length - 1]?.month
  const firstProj = projection[0]?.month
  const lastProj = projection[projection.length - 1]?.month
  const parts = []
  if (lastActual) parts.push(`Actual — through ${lastActual}`)
  if (firstProj) parts.push(`Plan / Target – ${firstProj}${lastProj && lastProj !== firstProj ? `–${lastProj}` : ''}`)
  return parts.join('   ')
})

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
</script>

<template>
  <section class="panel" aria-label="R4 Cumulative Conformance Trend">
    <div class="panel-head panel-head--navy panel-head--numbered">
      <span class="panel-head__num">6</span>
      <span>R4 CUMULATIVE CONFORMANCE TREND (%)</span>
    </div>
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
              :stroke="v === 100 ? '#0B55B7' : '#edf0f7'"
              stroke-width="1"
            />
            <text v-for="v in gridLines" :key="`l${v}`" :x="PAD.l - 7" :y="y(v) + 3" class="axis-label" text-anchor="end">
              {{ v }}%
            </text>
          </g>

          <!-- posisi tetap di pojok atas (bukan relatif ke garis target) supaya tidak
               bertabrakan dengan label titik data saat proyeksi mencapai nilai target -->
          <text v-if="trend.targetLabel" :x="VB_W - PAD.r" :y="14" class="target-label" text-anchor="end">
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
            :stroke="PLAN_COLOR"
            stroke-width="2.5"
            stroke-dasharray="6 4"
            stroke-linejoin="round"
          />
          <path :d="actualPath" fill="none" :stroke="ACTUAL_COLOR" stroke-width="2.5" stroke-linejoin="round" />

          <!-- titik + label -->
          <g v-for="(p, i) in allPoints" :key="p.month">
            <circle
              :cx="x(i)"
              :cy="y(p.value)"
              :r="hover === i ? 6 : 4.5"
              :fill="p.projected ? PLAN_COLOR : ACTUAL_COLOR"
              stroke="#fff"
              stroke-width="2"
            />
            <text :x="x(i)" :y="y(p.value) - 11" class="point-label" :fill="p.projected ? PLAN_COLOR : ACTUAL_COLOR" text-anchor="middle">
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

      <div class="trend-caption">{{ captionText }}</div>
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
  font-size: 9.5px;
  font-weight: 700;
  fill: #0b55b7;
  font-family: inherit;
}

.point-label {
  font-size: 9.5px;
  font-weight: 800;
  font-family: inherit;
}

.trend-caption {
  text-align: center;
  font-size: 0.64rem;
  font-weight: 700;
  color: var(--ink-muted);
  padding-top: 0.15rem;
}
</style>
