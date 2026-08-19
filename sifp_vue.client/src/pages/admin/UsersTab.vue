<script setup>
import { ref, onMounted } from 'vue'
import DataTable from '../../components/ui/DataTable.vue'
import DataState from '../../components/ui/DataState.vue'
import DashIcon from '../../components/dashboard/DashIcon.vue'
import { api } from '../../services/api'

const users = ref([])
const roles = ref([])
const loading = ref(true)
const error = ref('')

const showAddModal = ref(false)
const submitting = ref(false)
const formError = ref('')

const newUser = ref({
  username: '',
  email: '',
  fullName: '',
  password: '',
  roleId: null,
  zona: 1
})

const columns = [
  { key: 'id', label: 'ID', nowrap: true },
  { key: 'username', label: 'Username', nowrap: true },
  { key: 'fullName', label: 'Nama Lengkap', nowrap: true },
  { key: 'email', label: 'Email', nowrap: true },
  { key: 'roles', label: 'Role', nowrap: true },
  { key: 'zona', label: 'Zona', align: 'center' },
  { key: 'lastLoginAt', label: 'Login Terakhir', nowrap: true },
  { key: 'mfaEnabled', label: 'MFA', align: 'center' },
  { key: 'isActive', label: 'Status', align: 'center' },
]

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const [resUsers, resRoles] = await Promise.all([
      api.get('/api/users'),
      api.get('/api/users/roles')
    ])
    users.value = resUsers.items || resUsers || []
    roles.value = resRoles || []
    if (roles.value.length > 0) {
      newUser.value.roleId = roles.value[0].id
    }
  } catch (err) {
    error.value = err.message || 'Gagal memuat data pengguna.'
  } finally {
    loading.value = false
  }
}

async function handleCreateUser() {
  submitting.value = true
  formError.value = ''
  try {
    await api.post('/api/users', {
      username: newUser.value.username,
      email: newUser.value.email,
      fullName: newUser.value.fullName,
      password: newUser.value.password,
      zona: Number(newUser.value.zona || 1),
      roleIds: [newUser.value.roleId]
    })
    showAddModal.value = false
    newUser.value = { username: '', email: '', fullName: '', password: '', roleId: roles.value[0]?.id, zona: 1 }
    await loadData()
  } catch (err) {
    formError.value = err.message || 'Gagal membuat user baru.'
  } finally {
    submitting.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="admin-tab">
    <div class="tab-actions">
      <div>
        <h3 class="tab-title">Management Account & User Access</h3>
        <p class="tab-subtitle">Kelola akun pengguna, penugasan role, zona kerja, dan otentikasi MFA.</p>
      </div>
      <button type="button" class="btn btn-primary" @click="showAddModal = true">
        <DashIcon name="plus" :size="15" /> Tambah User Baru
      </button>
    </div>

    <DataState :loading="loading" :error="error" :empty="!users.length" @retry="loadData">
      <DataTable
        :columns="columns"
        :rows="users"
        :initial-sort="{ key: 'username', dir: 'asc' }"
      >
        <template #cell-roles="{ value }">
          <span v-for="r in (value || [])" :key="r" class="chip chip--accent">{{ r }}</span>
        </template>
        <template #cell-zona="{ value }">
          <span class="chip">Z{{ value || 1 }}</span>
        </template>
        <template #cell-mfaEnabled="{ value }">
          <span :class="['pill', value ? 'pill--yes' : 'pill--na']">{{ value ? 'AKTIF' : 'OFF' }}</span>
        </template>
        <template #cell-isActive="{ value }">
          <span :class="['pill', value ? 'pill--progress' : 'pill--no']">{{ value ? 'Aktif' : 'Nonaktif' }}</span>
        </template>
      </DataTable>
    </DataState>

    <!-- Modal Tambah User -->
    <div v-if="showAddModal" class="modal-backdrop" @click.self="showAddModal = false">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Tambah Akun User Baru</h3>
          <button type="button" class="btn-close" @click="showAddModal = false">
            <DashIcon name="close" :size="16" />
          </button>
        </div>
        <form @submit.prevent="handleCreateUser" class="modal-body">
          <div v-if="formError" class="modal-error">
            <DashIcon name="warning" :size="16" /> {{ formError }}
          </div>
          <div class="form-group">
            <label>Username *</label>
            <input v-model="newUser.username" type="text" class="form-input" required placeholder="misal: haris.elfian" />
          </div>
          <div class="form-group">
            <label>Nama Lengkap *</label>
            <input v-model="newUser.fullName" type="text" class="form-input" required placeholder="Nama lengkap user" />
          </div>
          <div class="form-group">
            <label>Email Pertamina *</label>
            <input v-model="newUser.email" type="email" class="form-input" required placeholder="user@pertamina.com" />
          </div>
          <div class="form-group">
            <label>Password *</label>
            <input v-model="newUser.password" type="password" class="form-input" required placeholder="Minimal 6 karakter" />
          </div>
          <div class="form-group">
            <label>Role Hak Akses *</label>
            <select v-model="newUser.roleId" class="form-input" required>
              <option v-for="r in roles" :key="r.id" :value="r.id">{{ r.name }} - {{ r.description }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Zona Kerja *</label>
            <input v-model.number="newUser.zona" type="number" min="1" max="99" class="form-input" required />
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-ghost" @click="showAddModal = false">Batal</button>
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              {{ submitting ? 'Menyimpan…' : 'Simpan User' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-tab {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}
.tab-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}
.tab-title {
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--ink);
  margin: 0;
}
.tab-subtitle {
  font-size: 0.76rem;
  color: var(--ink-muted);
  margin: 0.2rem 0 0;
}
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.78rem;
  font-weight: 800;
  cursor: pointer;
  border: 1px solid transparent;
}
.btn-primary { background: var(--navy-bar); color: #fff; }
.btn-ghost { background: #fff; border-color: var(--line); color: var(--ink); }
.modal-backdrop {
  position: fixed; inset: 0; background: rgba(15,23,42,0.55); backdrop-filter: blur(4px);
  z-index: 999; display: flex; align-items: center; justify-content: center; padding: 1rem;
}
.modal-card { background: #fff; border-radius: 14px; width: 100%; max-width: 500px; padding: 1.2rem; }
.modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
.modal-header h3 { font-size: 1rem; font-weight: 800; margin: 0; }
.btn-close { background: none; border: none; cursor: pointer; color: var(--ink-muted); }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; margin-bottom: 0.8rem; }
.form-group label { font-size: 0.72rem; font-weight: 700; }
.form-input { border: 1px solid var(--line); border-radius: 8px; padding: 0.45rem 0.65rem; font-size: 0.78rem; }
.modal-footer { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem; }
.modal-error { padding: 0.5rem; background: #fdeae8; color: #b3261e; border-radius: 6px; font-size: 0.75rem; margin-bottom: 0.8rem; }
</style>
