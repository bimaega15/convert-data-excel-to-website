<script setup>
import DashboardHeader from './components/dashboard/DashboardHeader.vue'
import GaugeCard from './components/dashboard/GaugeCard.vue'
import ConformanceCard from './components/dashboard/ConformanceCard.vue'
import QuickFacts from './components/dashboard/QuickFacts.vue'
import HealthMap from './components/dashboard/HealthMap.vue'
import TopFivePanel from './components/dashboard/TopFivePanel.vue'
import TrendChart from './components/dashboard/TrendChart.vue'
import ZonaChart from './components/dashboard/ZonaChart.vue'
import InitiativesTable from './components/dashboard/InitiativesTable.vue'
import ExecutiveSummary from './components/dashboard/ExecutiveSummary.vue'
import DashIcon from './components/dashboard/DashIcon.vue'
import { kpis, topPanels, footerNote } from './data/dashboard'
</script>

<template>
  <div class="dash">
    <DashboardHeader />

    <div class="dash-row dash-row--kpi">
      <GaugeCard v-for="kpi in kpis" :key="kpi.code" :kpi="kpi" />
      <ConformanceCard />
      <QuickFacts />
    </div>

    <div class="dash-row dash-row--mid">
      <HealthMap class="dash-healthmap" />
      <TopFivePanel v-for="panel in topPanels" :key="panel.no" :panel="panel" />
    </div>

    <div class="dash-row dash-row--charts">
      <TrendChart />
      <ZonaChart />
      <InitiativesTable class="dash-initiatives" />
    </div>

    <ExecutiveSummary />

    <div class="dash-note">
      <span class="dash-note__icon"><DashIcon name="target" :size="24" /></span>
      <p><strong>Note:</strong> {{ footerNote }}</p>
    </div>
  </div>
</template>

<style scoped>
.dash {
  max-width: 1760px;
  margin: 0 auto;
  padding: 0.9rem 1rem 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.dash-row {
  display: grid;
  gap: 0.8rem;
}

.dash-row--kpi {
  grid-template-columns: repeat(3, 1fr) 1.08fr 1.2fr;
}

.dash-row--mid {
  grid-template-columns: 1.85fr repeat(4, 1fr);
}

.dash-row--charts {
  grid-template-columns: 1.15fr 0.95fr 1.55fr;
}

.dash-note {
  display: flex;
  align-items: flex-start;
  gap: 0.7rem;
  padding: 0 0.4rem;
  color: var(--navy-bar);
}

.dash-note p {
  margin: 0;
  font-size: 0.68rem;
  font-weight: 600;
  line-height: 1.5;
  color: var(--ink);
  max-width: 640px;
}

.dash-note__icon {
  flex: none;
  margin-top: -0.05rem;
}

@media (max-width: 1500px) {
  .dash-row--kpi {
    grid-template-columns: repeat(3, 1fr);
  }

  .dash-row--mid {
    grid-template-columns: repeat(2, 1fr);
  }

  .dash-healthmap {
    grid-column: 1 / -1;
  }

  .dash-row--charts {
    grid-template-columns: 1fr 1fr;
  }

  .dash-initiatives {
    grid-column: 1 / -1;
  }
}

@media (max-width: 900px) {
  .dash-row--kpi,
  .dash-row--mid,
  .dash-row--charts {
    grid-template-columns: 1fr;
  }
}
</style>
