<script setup>
import DashIcon from './DashIcon.vue'
import { quickFacts } from '../../data/dashboard'
</script>

<template>
  <section class="panel" aria-label="Quick Facts">
    <div class="qf-head">QUICK FACTS</div>
    <ul class="qf-list">
      <li v-for="fact in quickFacts" :key="fact.label">
        <span class="qf-icon"><DashIcon :name="fact.icon" :size="17" /></span>
        <span class="qf-label">{{ fact.label }}</span>
        <span class="qf-value">{{ fact.value }}</span>
      </li>
    </ul>
  </section>
</template>

<style scoped>
.qf-head {
  align-self: flex-start;
  background: var(--navy-bar);
  color: #fff;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  padding: 0.35rem 1rem;
  border-radius: 0 0 10px 0;
}

.qf-list {
  list-style: none;
  margin: 0;
  padding: 0.5rem 0.85rem 0.7rem;
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-around;
  gap: 0.25rem;
}

.qf-list li {
  display: flex;
  align-items: center;
  gap: 0.55rem;
}

.qf-icon {
  flex: none;
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;
  border-radius: 8px;
  background: #eef1fb;
  color: var(--navy-bar);
}

.qf-label {
  flex: 1;
  font-size: 0.7rem;
  font-weight: 600;
  line-height: 1.2;
  color: var(--ink);
}

.qf-value {
  font-size: 0.74rem;
  font-weight: 800;
  color: var(--ink-strong);
  white-space: nowrap;
}
</style>
