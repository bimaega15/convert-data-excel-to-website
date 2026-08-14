<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DashIcon from '../dashboard/DashIcon.vue'
import { authState, clearSession } from '../../services/auth'

const props = defineProps({
  collapsed: { type: Boolean, default: false },
})

const emit = defineEmits(['toggle'])
const route = useRoute()
const router = useRouter()

const burgerLabel = computed(() =>
  props.collapsed ? 'Perlebar sidebar' : 'Perkecil sidebar menjadi ikon'
)

const displayName = computed(() => authState.user?.fullName || authState.user?.username || 'Pengguna')
const displayRole = computed(() => authState.user?.roles?.[0] ?? '')

function logout() {
  clearSession()
  router.push({ name: 'login' })
}
</script>

<template>
  <header class="topbar">
    <button
      type="button"
      class="topbar__burger"
      :aria-label="burgerLabel"
      :title="burgerLabel"
      :aria-expanded="!collapsed"
      @click="emit('toggle')"
    >
      <DashIcon name="menu" :size="20" />
    </button>

    <div class="topbar__title">
      <small>{{ route.meta.subtitle }}</small>
      <h1>{{ route.meta.title }}</h1>
    </div>

    <div class="topbar__right">
      <div class="dropdown">
        <button
          type="button"
          class="topbar__user"
          data-bs-toggle="dropdown"
          aria-expanded="false"
          :title="`Login sebagai ${displayName}`"
        >
          <span class="topbar__user-icon"><DashIcon name="person" :size="16" /></span>
          <span class="topbar__user-meta d-none d-md-flex">
            <strong>{{ displayName }}</strong>
            <small>{{ displayRole }}</small>
          </span>
        </button>
        <ul class="dropdown-menu dropdown-menu-end">
          <li><button type="button" class="dropdown-item" @click="logout">Keluar</button></li>
        </ul>
      </div>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  position: sticky;
  top: 0;
  z-index: 1030;
  display: flex;
  align-items: center;
  gap: 0.9rem;
  background: var(--surface);
  border-bottom: 1px solid var(--line);
  padding: 0.6rem 1.25rem;
  min-height: 58px;
}

.topbar__burger {
  background: none;
  border: 1px solid var(--line);
  border-radius: 10px;
  color: var(--ink);
  padding: 0.3rem 0.45rem;
  display: inline-flex;
  transition: background 0.15s, border-color 0.15s, color 0.15s;
}

.topbar__burger:hover {
  background: #f1f3fa;
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

.topbar__title small {
  display: block;
  font-size: 0.6rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--ink-muted);
}

.topbar__title h1 {
  margin: 0;
  font-size: 1.02rem;
  font-weight: 800;
  color: var(--ink-strong);
  line-height: 1.2;
}

.topbar__right {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 0.7rem;
}

.topbar__user {
  display: inline-flex;
  align-items: center;
  gap: 0.55rem;
  background: #f1f3fa;
  border: 1px solid var(--line);
  border-radius: 999px;
  padding: 0.25rem 0.75rem 0.25rem 0.3rem;
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
}

.topbar__user:hover,
.topbar__user[aria-expanded='true'] {
  background: #e6eaf6;
  border-color: var(--accent-blue);
}

.topbar__user-icon {
  flex: none;
  width: 30px;
  height: 30px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: linear-gradient(135deg, #1e2f83, #1d40b0);
  color: #fff;
}

.topbar__user-meta {
  flex-direction: column;
  line-height: 1.15;
  min-width: 0;
}

.topbar__user-meta strong {
  font-size: 0.72rem;
  font-weight: 800;
  color: var(--ink-strong);
  white-space: nowrap;
}

.topbar__user-meta small {
  font-size: 0.6rem;
  font-weight: 700;
  color: var(--ink-muted);
}

</style>
