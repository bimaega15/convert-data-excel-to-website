<script setup>
import { meta } from '../../data/dashboard'
</script>

<template>
  <header class="dash-header">
    <div class="dash-header__logos">
      <span class="logo-chip">SKK Migas</span>
      <span class="logo-chip">IOG 4.0</span>
      <span class="logo-chip">Energizing<br />Indonesia</span>
    </div>

    <div class="dash-header__title">
      <h1>{{ meta.title }}</h1>
      <p>{{ meta.subtitle }}</p>
    </div>

    <div class="dash-header__right">
      <span v-if="meta.draft" class="draft-stamp">DRAFT</span>
      <span class="brand-mark">
        <span class="brand-mark__name">PERTAMINA</span>
        <span class="brand-mark__unit">EP CEPU</span>
      </span>
    </div>
  </header>
</template>

<style scoped>
.dash-header {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  align-items: center;
  gap: 1rem;
  padding: 0.25rem 0.25rem 0.5rem;
}

.dash-header__logos {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.logo-chip {
  background: var(--surface);
  border: 1px solid var(--line);
  border-radius: 10px;
  padding: 0.35rem 0.6rem;
  font-size: 0.62rem;
  font-weight: 700;
  color: var(--ink-muted);
  text-align: center;
  line-height: 1.15;
}

.dash-header__title {
  text-align: center;
}

.dash-header__title h1 {
  margin: 0;
  font-size: clamp(1.25rem, 2.6vw, 2.1rem);
  font-weight: 800;
  letter-spacing: 0.02em;
  color: var(--ink-strong);
}

.dash-header__title p {
  margin: 0.1rem 0 0;
  font-size: clamp(0.78rem, 1.3vw, 1rem);
  font-weight: 700;
  color: var(--accent-blue);
}

.dash-header__right {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 1rem;
}

.draft-stamp {
  color: var(--accent-red);
  border: 3px solid var(--accent-red);
  border-radius: 10px;
  padding: 0.15rem 0.8rem;
  font-size: 1.15rem;
  font-weight: 800;
  letter-spacing: 0.06em;
  transform: rotate(-4deg);
  opacity: 0.9;
}

.brand-mark {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  line-height: 1.1;
}

.brand-mark__name {
  font-size: 1.05rem;
  font-weight: 800;
  letter-spacing: 0.03em;
  color: var(--ink-strong);
}

.brand-mark__unit {
  font-size: 0.8rem;
  font-weight: 800;
  color: var(--accent-red);
  letter-spacing: 0.12em;
}

@media (max-width: 900px) {
  .dash-header {
    grid-template-columns: 1fr;
    justify-items: center;
    row-gap: 0.5rem;
  }

  .dash-header__right {
    justify-content: center;
  }
}
</style>
