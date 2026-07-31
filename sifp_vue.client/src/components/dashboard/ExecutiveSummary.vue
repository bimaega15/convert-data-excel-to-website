<script setup>
import DashIcon from './DashIcon.vue'
import { summaryCards, summaryNotes, statusLegend } from '../../data/dashboard'

const toneColor = {
  red: 'var(--accent-red)',
  navy: 'var(--navy-bar)',
  green: 'var(--accent-green)',
}
</script>

<template>
  <section class="panel es-panel" aria-label="Executive Summary">
    <div class="es-tab">9. EXECUTIVE SUMMARY – MANAGEMENT ATTENTION REQUIRED</div>

    <div class="es-grid">
      <article v-for="card in summaryCards" :key="card.title" class="es-card">
        <span class="es-icon" :style="{ color: toneColor[card.tone] }">
          <DashIcon :name="card.icon" :size="26" />
        </span>
        <div>
          <h3 :style="{ color: toneColor[card.tone] }">{{ card.title }}</h3>
          <p>{{ card.text }}</p>
        </div>
      </article>

      <article class="es-card">
        <div>
          <h3 class="es-h-navy">LEGEND (Conformance Score)</h3>
          <ul class="es-legend">
            <li v-for="item in statusLegend" :key="item.status">
              <span class="status-dot" :class="`status-dot--${item.status}`"></span>
              <span>{{ item.label }}</span>
            </li>
          </ul>
        </div>
      </article>

      <article class="es-card">
        <div>
          <h3 class="es-h-navy">NOTES</h3>
          <ul class="es-notes">
            <li v-for="note in summaryNotes" :key="note">{{ note }}</li>
          </ul>
        </div>
      </article>
    </div>
  </section>
</template>

<style scoped>
.es-tab {
  align-self: flex-start;
  background: var(--red-bar);
  color: #fff;
  font-size: 0.72rem;
  font-weight: 800;
  letter-spacing: 0.02em;
  padding: 0.4rem 1.1rem;
  border-radius: 0 0 12px 0;
}

.es-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 0.25rem;
  padding: 0.6rem 0.75rem 0.75rem;
}

.es-card {
  display: flex;
  gap: 0.55rem;
  padding: 0.2rem 0.65rem;
}

.es-card + .es-card {
  border-left: 1px solid var(--line);
}

.es-icon {
  flex: none;
  margin-top: 0.1rem;
}

.es-card h3 {
  margin: 0 0 0.3rem;
  font-size: 0.66rem;
  font-weight: 800;
  letter-spacing: 0.02em;
}

.es-h-navy {
  color: var(--navy-bar);
}

.es-card p {
  margin: 0;
  font-size: 0.63rem;
  font-weight: 600;
  line-height: 1.45;
  color: var(--ink);
}

.es-legend,
.es-notes {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.es-legend li {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.62rem;
  font-weight: 700;
  color: var(--ink);
}

.es-notes li {
  position: relative;
  padding-left: 0.75rem;
  font-size: 0.62rem;
  font-weight: 600;
  line-height: 1.4;
  color: var(--ink);
}

.es-notes li::before {
  content: '•';
  position: absolute;
  left: 0;
  color: var(--navy-bar);
  font-weight: 800;
}

@media (max-width: 1500px) {
  .es-grid {
    grid-template-columns: repeat(4, 1fr);
    gap: 0.6rem 0.25rem;
  }

  .es-card:nth-child(5) {
    border-left: none;
  }
}

@media (max-width: 900px) {
  .es-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .es-card {
    border-left: none !important;
    padding-left: 0.2rem;
  }
}

@media (max-width: 560px) {
  .es-grid {
    grid-template-columns: 1fr;
  }
}
</style>
