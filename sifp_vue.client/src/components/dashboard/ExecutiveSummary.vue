<script setup>
import DashIcon from './DashIcon.vue'
import { summaryCards } from '../../data/dashboard'

const toneColor = {
  red: '#E10B12',
  navy: 'var(--navy-bar)',
  green: '#07913B',
  blue: '#0B73C9',
  orange: '#E97700',
}
</script>

<template>
  <section class="panel es-panel" aria-label="Executive Summary">
    <div class="panel-head panel-head--navy panel-head--numbered">
      <span class="panel-head__num">9</span>
      <span>EXECUTIVE SUMMARY</span>
    </div>

    <ul class="es-list">
      <li v-for="card in summaryCards" :key="card.title">
        <span class="es-icon" :style="{ background: toneColor[card.tone] ?? toneColor.navy }">
          <DashIcon :name="card.icon" :size="20" />
        </span>
        <span class="es-label" :style="{ color: toneColor[card.tone] ?? toneColor.navy }">{{ card.title }}:</span>
        <span class="es-text">{{ card.text }}</span>
      </li>
    </ul>
  </section>
</template>

<style scoped>
.es-list {
  list-style: none;
  margin: 0;
  padding: 0.4rem 1rem 0.6rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.es-list li {
  display: flex;
  align-items: center;
  gap: 0.9rem;
  padding: 0.65rem 0;
  border-bottom: 1px solid var(--line-soft);
}

.es-list li:last-child {
  border-bottom: none;
}

.es-icon {
  flex: none;
  display: grid;
  place-items: center;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  color: #fff;
}

.es-label {
  flex: none;
  font-size: 0.78rem;
  font-weight: 800;
  white-space: nowrap;
}

.es-text {
  flex: 1;
  font-size: 0.76rem;
  font-weight: 500;
  line-height: 1.4;
  color: #222222;
}

@media (max-width: 900px) {
  .es-list li {
    flex-wrap: wrap;
  }
}
</style>
