<script setup>
import { computed } from 'vue'
import DashIcon from './DashIcon.vue'
import { quickFacts, zonaScores, zoneShort } from '../../data/dashboard'

// Bar ini butuh 5 slot tetap (beda dari daftar QuickFacts umum yang bisa berisi
// berapa pun butir) — dicari lewat cocok-label, bukan posisi array, supaya tidak
// rapuh kalau urutan/isi tabel QuickFacts di database berubah.
function pickFact(...keywords) {
  return quickFacts.find((f) =>
    keywords.some((k) => f.label?.toLowerCase().includes(k.toLowerCase()))
  )
}

const totalObs = computed(() => pickFact('total observations'))
const obsPeriod = computed(() => pickFact('observation period'))
const sites = computed(() => pickFact('sites', 'locations'))
const zonesCovered = computed(() => pickFact('zones covered'))

const zoneDistribution = computed(() =>
  zonaScores.bars.map((b) => `${zoneShort(b.zone)}  ${b.obs}`).join('   |   ')
)

const items = computed(() => [
  { icon: 'clipboard', headline: totalObs.value?.value, caption: 'Total Observations' },
  { icon: 'target', headline: zoneDistribution.value, singleLine: true },
  { icon: 'calendar', headline: obsPeriod.value?.value, caption: 'Observation Period:', reverse: true },
  { icon: 'pin', headline: sites.value?.value, caption: 'Sites / Locations' },
  { icon: 'layers', headline: zonesCovered.value?.value, caption: 'Zones Covered' },
])
</script>

<template>
  <section class="panel quick-facts" aria-label="Quick Facts">
    <template v-for="(item, i) in items" :key="item.caption ?? item.icon">
      <div v-if="i > 0" class="quick-facts__divider"></div>
      <div class="quick-facts__item">
        <span class="quick-facts__icon"><DashIcon :name="item.icon" :size="22" /></span>

        <span v-if="item.singleLine" class="quick-facts__single">{{ item.headline }}</span>
        <span v-else class="quick-facts__stack" :class="{ 'quick-facts__stack--reverse': item.reverse }">
          <span class="quick-facts__headline">{{ item.headline }}</span>
          <span class="quick-facts__caption">{{ item.caption }}</span>
        </span>
      </div>
    </template>
  </section>
</template>

<style scoped>
.quick-facts {
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  padding: 0.7rem 1.2rem;
  gap: 0.6rem;
}

.quick-facts__item {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  flex: 1;
  min-width: 0;
}

.quick-facts__divider {
  width: 1px;
  align-self: stretch;
  background: #8799ad;
  opacity: 0.4;
}

.quick-facts__icon {
  flex: none;
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 50%;
  background: var(--navy-bar);
  color: #fff;
}

.quick-facts__stack {
  display: flex;
  flex-direction: column;
  line-height: 1.25;
  min-width: 0;
}

.quick-facts__stack--reverse {
  flex-direction: column-reverse;
}

.quick-facts__headline {
  font-size: 1.3rem;
  font-weight: 800;
  color: #082c68;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.quick-facts__stack--reverse .quick-facts__headline {
  font-size: 0.85rem;
  font-weight: 600;
  color: #111111;
}

.quick-facts__caption {
  font-size: 0.68rem;
  font-weight: 700;
  color: #111111;
}

.quick-facts__single {
  font-size: 0.8rem;
  font-weight: 700;
  color: #111111;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

@media (max-width: 1100px) {
  .quick-facts {
    flex-wrap: wrap;
  }

  .quick-facts__item {
    flex: 1 1 40%;
  }

  .quick-facts__divider {
    display: none;
  }
}
</style>
