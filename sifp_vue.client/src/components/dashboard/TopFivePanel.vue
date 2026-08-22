<script setup>
import { computed } from 'vue'

const props = defineProps({
  panel: { type: Object, required: true },
})

const dashColor = {
  green: 'var(--st-effective)',
  red: 'var(--st-failed)',
  amber: 'var(--st-degraded)',
}

// Cuma panel Drift (4) & Systemic Issues (5) yang menampilkan total di judul,
// mengikuti desain slide — SIF Exposure & Safeguard Gap tidak.
const titleSuffix = computed(() => {
  const total = props.panel.footer?.value
  if (!total) return ''
  if (props.panel.no === 4) return ` (Total ${total})`
  if (props.panel.no === 5) return ` (${total})`
  return ''
})

// lebar dash proporsional terhadap bobot item (min. tetap terlihat)
function dashWidth(weight) {
  return `${10 + weight * 24}px`
}
</script>

<template>
  <section class="panel" :aria-label="panel.title">
    <div class="panel-head panel-head--navy panel-head--numbered">
      <span class="panel-head__num">{{ panel.no }}</span>
      <span>{{ panel.title }}{{ titleSuffix }}</span>
    </div>

    <ol class="t5-list">
      <li v-for="(item, i) in panel.items" :key="i">
        <span class="t5-rank">{{ i + 1 }}.</span>
        <span class="t5-label">{{ item.label }}</span>
        <span class="t5-value">
          <span
            v-if="panel.dash && item.weight > 0"
            class="t5-dash"
            :style="{ width: dashWidth(item.weight), background: dashColor[panel.dash] }"
          ></span>
          <span class="t5-num">{{ item.display }}</span>
        </span>
      </li>
    </ol>
  </section>
</template>

<style scoped>
.t5-list {
  list-style: none;
  margin: 0;
  padding: 0.5rem 0.9rem;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.t5-list li {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--line-soft);
}

.t5-list li:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.t5-rank {
  flex: none;
  font-size: 0.72rem;
  font-weight: 600;
  color: #111111;
  width: 1rem;
}

.t5-label {
  flex: 1;
  font-size: 0.72rem;
  font-weight: 600;
  line-height: 1.25;
  color: #222222;
}

.t5-value {
  flex: none;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}

.t5-dash {
  height: 6px;
  border-radius: 4px;
}

.t5-num {
  font-size: 0.72rem;
  font-weight: 800;
  color: var(--navy-bar);
  white-space: nowrap;
}
</style>
