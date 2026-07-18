<script setup>
import DashIcon from './DashIcon.vue'

const props = defineProps({
  panel: { type: Object, required: true },
})

const dashColor = {
  green: 'var(--st-effective)',
  red: 'var(--st-failed)',
  amber: 'var(--st-degraded)',
}

const footerIconColor = {
  green: 'var(--accent-green)',
  red: 'var(--accent-red)',
  blue: 'var(--accent-blue)',
  purple: 'var(--accent-purple)',
}

// lebar dash proporsional terhadap bobot item (min. tetap terlihat)
function dashWidth(weight) {
  return `${10 + weight * 24}px`
}
</script>

<template>
  <section class="panel" :aria-label="panel.title">
    <div class="panel-head" :class="`panel-head--${panel.variant}`">
      {{ panel.no }}. {{ panel.title }}
      <small>{{ panel.subtitle }}</small>
    </div>

    <ol class="t5-list">
      <li v-for="(item, i) in panel.items" :key="i">
        <span class="t5-rank">{{ i + 1 }}</span>
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

    <div class="t5-footer">
      <span class="t5-footer-icon" :style="{ color: footerIconColor[panel.variant] }">
        <DashIcon :name="panel.footer.icon" :size="17" />
      </span>
      <span>{{ panel.footer.label }}: {{ panel.footer.value }}</span>
    </div>
  </section>
</template>

<style scoped>
.t5-list {
  list-style: none;
  margin: 0;
  padding: 0.6rem 0.8rem;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
}

.t5-list li {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
}

.t5-rank {
  flex: none;
  font-size: 0.7rem;
  font-weight: 800;
  color: var(--ink);
  width: 0.9rem;
}

.t5-label {
  flex: 1;
  font-size: 0.7rem;
  font-weight: 700;
  line-height: 1.25;
  color: var(--ink);
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
  font-size: 0.7rem;
  font-weight: 800;
  color: var(--ink);
  white-space: nowrap;
}

.t5-footer {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  border-top: 1px solid var(--line);
  padding: 0.5rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 800;
  color: var(--ink);
}

.t5-footer-icon {
  display: inline-flex;
}
</style>
