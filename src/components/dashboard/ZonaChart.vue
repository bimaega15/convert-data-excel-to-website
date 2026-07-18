<script setup>
import { computed, ref } from 'vue'
import { zonaScores } from '../../data/dashboard'

const VB_W = 420
const VB_H = 250
const PAD = { l: 42, r: 14, t: 30, b: 44 }
const plotW = VB_W - PAD.l - PAD.r
const plotH = VB_H - PAD.t - PAD.b
const BAR_W = 42

const slotX = (i) => PAD.l + ((i + 0.5) * plotW) / zonaScores.bars.length
const y = (v) => PAD.t + (1 - v / 100) * plotH
const baseline = VB_H - PAD.b

const gridLines = [0, 25, 50, 75, 100]

function barColor(v) {
  if (v < 50) return 'var(--st-failed)'
  if (v < 80) return 'var(--st-degraded)'
  return 'var(--st-effective)'
}

// bar dengan sudut atas membulat 4px, menempel ke baseline
function barPath(i, v) {
  const xLeft = slotX(i) - BAR_W / 2
  const top = y(v)
  const r = Math.min(4, baseline - top)
  return [
    `M ${xLeft} ${baseline}`,
    `L ${xLeft} ${top + r}`,
    `Q ${xLeft} ${top} ${xLeft + r} ${top}`,
    `L ${xLeft + BAR_W - r} ${top}`,
    `Q ${xLeft + BAR_W} ${top} ${xLeft + BAR_W} ${top + r}`,
    `L ${xLeft + BAR_W} ${baseline}`,
    'Z',
  ].join(' ')
}

const hover = ref(null)

const tipStyle = computed(() => {
  if (hover.value == null) return {}
  const bar = zonaScores.bars[hover.value]
  return {
    left: `${(slotX(hover.value) / VB_W) * 100}%`,
    top: `${(y(Math.max(bar.value, 8)) / VB_H) * 100}%`,
  }
})
</script>

<template>
  <section class="panel" aria-label="Conformance Score by Zona">
    <div class="panel-head panel-head--navy">7. CONFORMANCE SCORE BY ZONA (Latest)</div>
    <div class="panel-body zona-body">
      <div class="chart-wrap">
        <svg :viewBox="`0 0 ${VB_W} ${VB_H}`" role="img" aria-label="Bar chart conformance score per zona">
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

          <line
            :x1="PAD.l"
            :x2="VB_W - PAD.r"
            :y1="y(zonaScores.target)"
            :y2="y(zonaScores.target)"
            stroke="var(--st-effective)"
            stroke-width="1.6"
            stroke-dasharray="6 4"
          />
          <text :x="VB_W - PAD.r" :y="y(zonaScores.target) - 7" class="target-label" text-anchor="end">
            {{ zonaScores.targetLabel }}
          </text>

          <g
            v-for="(bar, i) in zonaScores.bars"
            :key="bar.zone"
            @mouseenter="hover = i"
            @mouseleave="hover = null"
          >
            <!-- area hover lebih besar dari mark -->
            <rect
              :x="slotX(i) - BAR_W"
              :y="PAD.t"
              :width="BAR_W * 2"
              :height="plotH"
              fill="transparent"
            />
            <path
              v-if="bar.value > 0"
              :d="barPath(i, bar.value)"
              :fill="barColor(bar.value)"
              :opacity="hover == null || hover === i ? 1 : 0.55"
            />
            <text :x="slotX(i)" :y="y(bar.value) - 8" class="point-label" text-anchor="middle">
              {{ bar.value.toFixed(2) }}%
            </text>
            <text :x="slotX(i)" :y="baseline + 16" class="axis-label" text-anchor="middle">
              {{ bar.zone }}
            </text>
            <text :x="slotX(i)" :y="baseline + 29" class="axis-sub" text-anchor="middle">
              ({{ bar.obs }} Obs)
            </text>
          </g>
        </svg>

        <div v-if="hover != null" class="chart-tip" :style="tipStyle">
          {{ zonaScores.bars[hover].zone }} ({{ zonaScores.bars[hover].obs }} Obs) ·
          {{ zonaScores.bars[hover].value.toFixed(2) }}%
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.zona-body {
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

.axis-sub {
  font-size: 9.5px;
  font-weight: 600;
  fill: var(--ink-muted);
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
