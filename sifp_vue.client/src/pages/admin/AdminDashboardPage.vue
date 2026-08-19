<script setup>
import { ref } from 'vue'
import PageHeader from '../../components/ui/PageHeader.vue'
import UsersTab from './UsersTab.vue'
import RolesTab from './RolesTab.vue'
import LogsTab from './LogsTab.vue'
import DashIcon from '../../components/dashboard/DashIcon.vue'

const activeTab = ref('users')

const tabs = [
  { id: 'users', label: 'User Management', icon: 'person' },
  { id: 'roles', label: 'Roles & Access Menu', icon: 'shield' },
  { id: 'logs', label: 'Log System for Maintenance', icon: 'checklist' },
]
</script>

<template>
  <div class="admin-dashboard-page">
    <PageHeader
      title="Admin Dashboard & Control Center"
      subtitle="Pusat kontrol administrator untuk mengelola User, Role & Access Menu, Master Data, dan Pemantauan System Log."
    >
      <template #right>
        <div class="admin-master-links">
          <router-link to="/master/observations" class="master-chip">Observations</router-link>
          <router-link to="/master/initiatives" class="master-chip">Initiatives</router-link>
          <router-link to="/master/sif-questions" class="master-chip">SIF Questions</router-link>
          <router-link to="/master/ccvc-library" class="master-chip">CCVC Library</router-link>
        </div>
      </template>
    </PageHeader>

    <div class="admin-tabs-nav">
      <button
        v-for="t in tabs"
        :key="t.id"
        type="button"
        :class="['tab-btn', { 'tab-btn--active': activeTab === t.id }]"
        @click="activeTab = t.id"
      >
        <DashIcon :name="t.icon" :size="16" />
        <span>{{ t.label }}</span>
      </button>
    </div>

    <div class="admin-tab-content">
      <UsersTab v-if="activeTab === 'users'" />
      <RolesTab v-else-if="activeTab === 'roles'" />
      <LogsTab v-else-if="activeTab === 'logs'" />
    </div>
  </div>
</template>

<style scoped>
.admin-dashboard-page {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}

.admin-master-links {
  display: flex;
  gap: 0.4rem;
  flex-wrap: wrap;
}

.master-chip {
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--accent-blue);
  background: #f0f4ff;
  border: 1px solid #d0dcfb;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  text-decoration: none;
}

.master-chip:hover {
  background: var(--accent-blue);
  color: #fff;
}

.admin-tabs-nav {
  display: flex;
  gap: 0.5rem;
  border-bottom: 2px solid var(--line);
  padding-bottom: 0.1rem;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.65rem 1.1rem;
  border: none;
  background: transparent;
  font-size: 0.8rem;
  font-weight: 800;
  color: var(--ink-muted);
  cursor: pointer;
  border-bottom: 3px solid transparent;
  transition: all 0.15s;
}

.tab-btn:hover {
  color: var(--ink);
}

.tab-btn--active {
  color: var(--navy-bar);
  border-bottom-color: var(--navy-bar);
  background: #ffffff;
  border-radius: 8px 8px 0 0;
}

.admin-tab-content {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid var(--line);
  padding: 1.2rem;
}
</style>
