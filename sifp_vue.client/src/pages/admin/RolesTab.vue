<script setup>
import { ref, onMounted } from 'vue'
import DataState from '../../components/ui/DataState.vue'
import DashIcon from '../../components/dashboard/DashIcon.vue'
import { api } from '../../services/api'

const roles = ref([])
const permissions = ref([])
const loading = ref(true)
const error = ref('')

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const [resRoles, resPerms] = await Promise.all([
      api.get('/api/roles'),
      api.get('/api/roles/permissions')
    ])
    roles.value = resRoles || []
    permissions.value = resPerms || []
  } catch (err) {
    error.value = err.message || 'Gagal memuat matriks hak akses role.'
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="admin-tab">
    <div class="tab-header">
      <h3 class="tab-title">Role Action & Menu Access Permission</h3>
      <p class="tab-subtitle">Konfigurasi hak akses menu dan aksi pengguna berdasarkan Role (Administrator, Verifier, Viewer).</p>
    </div>

    <DataState :loading="loading" :error="error" @retry="loadData">
      <div class="roles-grid">
        <div v-for="r in roles" :key="r.id" class="role-card">
          <div class="role-card__header">
            <span class="role-badge">{{ r.name }}</span>
            <span class="user-count">{{ r.userCount }} Pengguna</span>
          </div>
          <p class="role-desc">{{ r.description || 'Hak akses pengguna aplikasi SIFP Assurance.' }}</p>
          <div class="role-access">
            <span :class="['access-flag', r.canAccessAdmin ? 'access-flag--yes' : 'access-flag--no']">
              <DashIcon :name="r.canAccessAdmin ? 'shield' : 'close'" :size="14" />
              Akses Admin MVC: {{ r.canAccessAdmin ? 'Ya' : 'Tidak' }}
            </span>
          </div>
        </div>
      </div>

      <div class="matrix-card mt-4">
        <h4 class="matrix-title">Matriks Hak Akses Menu & Fitur</h4>
        <div class="table-responsive">
          <table class="perm-table">
            <thead>
              <tr>
                <th>Menu / Fitur Utama</th>
                <th class="text-center">Administrator</th>
                <th class="text-center">Verifier</th>
                <th class="text-center">Viewer</th>
                <th class="text-center">Aksi Tambah/Edit</th>
                <th class="text-center">Aksi Hapus</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in permissions" :key="p.menu">
                <td><strong>{{ p.menu }}</strong></td>
                <td class="text-center">
                  <span :class="['check-icon', p.roles.includes('Administrator') ? 'check-yes' : 'check-no']">
                    {{ p.roles.includes('Administrator') ? '✓' : '✗' }}
                  </span>
                </td>
                <td class="text-center">
                  <span :class="['check-icon', p.roles.includes('Verifier') ? 'check-yes' : 'check-no']">
                    {{ p.roles.includes('Verifier') ? '✓' : '✗' }}
                  </span>
                </td>
                <td class="text-center">
                  <span :class="['check-icon', p.roles.includes('Viewer') ? 'check-yes' : 'check-no']">
                    {{ p.roles.includes('Viewer') ? '✓' : '✗' }}
                  </span>
                </td>
                <td class="text-center">
                  <span :class="['chip', p.canCreate ? 'chip--accent' : '']">{{ p.canCreate ? 'Diizinkan' : 'Dibatasi' }}</span>
                </td>
                <td class="text-center">
                  <span :class="['pill', p.canDelete ? 'pill--progress' : 'pill--no']">{{ p.canDelete ? 'Diizinkan' : 'Dibatasi' }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </DataState>
  </div>
</template>

<style scoped>
.admin-tab { display: flex; flex-direction: column; gap: 1rem; }
.tab-title { font-size: 1.1rem; font-weight: 800; color: var(--ink); margin: 0; }
.tab-subtitle { font-size: 0.76rem; color: var(--ink-muted); margin: 0.2rem 0 0; }
.roles-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 1rem; margin-top: 0.5rem; }
.role-card { background: #fff; border: 1px solid var(--line); border-radius: 12px; padding: 1.1rem; }
.role-card__header { display: flex; justify-content: space-between; align-items: center; }
.role-badge { font-weight: 800; color: var(--navy-bar); font-size: 0.9rem; }
.user-count { font-size: 0.72rem; color: var(--ink-muted); font-weight: 700; }
.role-desc { font-size: 0.74rem; color: var(--ink); margin: 0.6rem 0; line-height: 1.4; }
.access-flag { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.72rem; font-weight: 700; border-radius: 6px; padding: 0.3rem 0.5rem; }
.access-flag--yes { background: #eaf1ff; color: var(--accent-blue); }
.access-flag--no { background: #fdeae8; color: #b3261e; }
.matrix-card { background: #fff; border: 1px solid var(--line); border-radius: 12px; padding: 1.1rem; }
.matrix-title { font-size: 0.92rem; font-weight: 800; margin-bottom: 0.8rem; }
.perm-table { width: 100%; border-collapse: collapse; font-size: 0.76rem; }
.perm-table th, .perm-table td { padding: 0.6rem 0.8rem; border-bottom: 1px solid var(--line-soft); }
.perm-table th { background: #f8fafc; font-size: 0.66rem; text-transform: uppercase; font-weight: 800; }
.text-center { text-align: center; }
.check-icon { font-weight: 900; font-size: 0.9rem; }
.check-yes { color: var(--accent-green); }
.check-no { color: #ccc; }
</style>
