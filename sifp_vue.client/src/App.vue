<script setup>
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AdminSidebar from './components/layout/AdminSidebar.vue'
import AdminTopbar from './components/layout/AdminTopbar.vue'

const route = useRoute()

// Di layar kecil sidebar jadi off-canvas; di desktop ia menyusut jadi ikon saja.
const collapsed = ref(Boolean(route.meta.collapseSidebar))
const mobileOpen = ref(false)

const isMobile = () => window.matchMedia('(max-width: 991.98px)').matches

// Setiap pindah halaman, lebar sidebar kembali ke preferensi bawaan route
// (dashboard butuh ruang lebar sehingga default-nya collapsed).
watch(
  () => route.name,
  () => {
    mobileOpen.value = false
    collapsed.value = Boolean(route.meta.collapseSidebar)
  }
)

function toggleSidebar() {
  if (isMobile()) mobileOpen.value = !mobileOpen.value
  else collapsed.value = !collapsed.value
}
</script>

<template>
  <router-view v-if="route.meta.public" />
  <div v-else class="admin" :class="{ 'admin--collapsed': collapsed }">
    <AdminSidebar :collapsed="collapsed" :open="mobileOpen" @close="mobileOpen = false" />
    <div v-if="mobileOpen" class="admin__backdrop d-lg-none" @click="mobileOpen = false"></div>

    <div class="admin__main">
      <AdminTopbar :collapsed="collapsed" @toggle="toggleSidebar" />
      <main class="admin__content">
        <router-view />
      </main>
    </div>
  </div>
</template>

<style scoped>
.admin {
  min-height: 100vh;
}

.admin__main {
  margin-left: 264px;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  transition: margin-left 0.22s ease;
}

.admin--collapsed .admin__main {
  margin-left: 76px;
}

.admin__content {
  flex: 1;
}

.admin__backdrop {
  position: fixed;
  inset: 0;
  background: rgba(10, 18, 50, 0.5);
  z-index: 1035;
}

@media (max-width: 991.98px) {
  .admin__main,
  .admin--collapsed .admin__main {
    margin-left: 0;
  }
}
</style>
