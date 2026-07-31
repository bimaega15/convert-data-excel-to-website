<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import DashIcon from '../dashboard/DashIcon.vue'
import { meta } from '../../data/dashboard'
import { currentUser, isLoggedIn, logout, userRoles } from '../../services/auth'

const props = defineProps({
  collapsed: { type: Boolean, default: false },
})

const emit = defineEmits(['toggle'])
const route = useRoute()

const burgerLabel = computed(() =>
  props.collapsed ? 'Perlebar sidebar' : 'Perkecil sidebar menjadi ikon'
)

// Inisial dari nama lengkap; jatuh ke dua huruf pertama username bila kosong.
const initials = computed(() => {
  const user = currentUser.value
  if (!user) return 'VV'

  const source = user.fullName?.trim() || user.username || ''
  const parts = source.split(/\s+/).filter(Boolean)
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
  return source.slice(0, 2).toUpperCase() || 'VV'
})
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
      <span class="topbar__chip d-none d-md-inline-flex" :title="meta.sourceFile">
        <DashIcon name="file" :size="14" />
        <span>{{ meta.sourceFile || 'Belum ada import' }}</span>
      </span>

      <template v-if="isLoggedIn">
        <button type="button" class="topbar__logout" title="Keluar dari sesi" @click="logout()">
          Keluar
        </button>
        <span class="topbar__avatar" :title="`${currentUser?.fullName || currentUser?.username} · ${userRoles.join(', ')}`">
          {{ initials }}
        </span>
      </template>
      <RouterLink v-else to="/login" class="topbar__login">Masuk</RouterLink>
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

.topbar__chip {
  align-items: center;
  gap: 0.4rem;
  max-width: 340px;
  background: #f1f3fa;
  border: 1px solid var(--line);
  border-radius: 999px;
  padding: 0.28rem 0.75rem;
  font-size: 0.64rem;
  font-weight: 700;
  color: var(--ink);
}

.topbar__chip span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.topbar__login,
.topbar__logout {
  flex: none;
  border: 1px solid var(--line);
  border-radius: 9px;
  background: #fff;
  padding: 0.3rem 0.75rem;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--ink);
  font-family: inherit;
  text-decoration: none;
  line-height: 1.6;
}

.topbar__login:hover {
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

.topbar__logout:hover {
  border-color: #d93025;
  color: #b3261e;
}

.topbar__avatar {
  flex: none;
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: linear-gradient(135deg, #1e2f83, #1d40b0);
  color: #fff;
  font-size: 0.72rem;
  font-weight: 800;
}
</style>
