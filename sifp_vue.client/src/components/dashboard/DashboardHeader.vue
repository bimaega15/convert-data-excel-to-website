<script setup>
import { computed } from 'vue'
import { meta } from '../../data/dashboard'
import skkLogo from '../../assets/images/SKK.png'
import iogLogo from '../../assets/images/IOG PNG.png'
import pertamina68Logo from '../../assets/images/Pertamina Logo 68 Final-Logo Alternatif.png'

const dataCutoff = computed(() => {
  if (!meta.generatedAt) return ''
  const d = new Date(meta.generatedAt)
  if (Number.isNaN(d.getTime())) return ''
  return `Data per ${d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}`
})
</script>

<template>
  <header class="dash-header">
    <div class="dash-header__logos">
      <img :src="skkLogo" alt="SKK Migas" class="dash-header__logo dash-header__logo--wide" />
      <img :src="iogLogo" alt="IOG 4.0" class="dash-header__logo" />
      <img :src="pertamina68Logo" alt="Pertamina 68 - Energizing Indonesia" class="dash-header__logo" />
    </div>

    <div class="dash-header__title">
      <h1>{{ meta.title }}</h1>
      <p>{{ dataCutoff }}</p>
    </div>

    <div class="dash-header__right">
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
  padding: 0.4rem 0.25rem 0.6rem;
}

.dash-header__logos {
  display: flex;
  align-items: center;
  gap: 1.1rem;
}

.dash-header__logo {
  height: 40px;
  width: auto;
  object-fit: contain;
}

.dash-header__logo--wide {
  height: 34px;
}

.dash-header__title {
  text-align: center;
}

.dash-header__title h1 {
  margin: 0;
  font-size: clamp(1.1rem, 2.4vw, 1.7rem);
  font-weight: 800;
  letter-spacing: 0.01em;
  color: #082c68;
}

.dash-header__title p {
  margin: 0.15rem 0 0;
  font-size: clamp(0.72rem, 1.1vw, 0.85rem);
  font-weight: 700;
  color: var(--ink);
}

.dash-header__right {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 1rem;
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
