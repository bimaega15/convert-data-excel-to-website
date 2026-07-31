<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '../components/ui/PageHeader.vue'
import DashIcon from '../components/dashboard/DashIcon.vue'
import { currentUser, isLoggedIn, login, logout } from '../services/auth'

const route = useRoute()
const router = useRouter()

const username = ref('')
const password = ref('')
const submitting = ref(false)
const errorMsg = ref('')

async function onSubmit() {
  errorMsg.value = ''
  submitting.value = true

  try {
    await login(username.value.trim(), password.value)
    password.value = ''

    // Kembali ke halaman yang tadi meminta login, mis. /import.
    const target = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    router.replace(target)
  } catch (err) {
    errorMsg.value = err.message
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Masuk"
      subtitle="Login diperlukan untuk aksi yang mengubah data, seperti import Excel. Dashboard dan tabel master bisa dibuka tanpa login."
    />

    <div v-if="isLoggedIn" class="panel login__status">
      <DashIcon name="checklist" :size="20" />
      <div class="login__status-body">
        <p class="login__status-title">
          Sudah masuk sebagai <strong>{{ currentUser?.fullName || currentUser?.username }}</strong>
        </p>
        <p class="login__status-role">
          Role: {{ currentUser?.roles?.join(', ') || '-' }}
        </p>
      </div>
      <button type="button" class="login__logout" @click="logout()">Keluar</button>
    </div>

    <form v-else class="panel login" @submit.prevent="onSubmit">
      <div v-if="errorMsg" class="login__error">{{ errorMsg }}</div>

      <label class="login__field">
        <span>Username</span>
        <input v-model="username" type="text" autocomplete="username" required autofocus />
      </label>

      <label class="login__field">
        <span>Password</span>
        <input v-model="password" type="password" autocomplete="current-password" required />
      </label>

      <button type="submit" class="login__submit" :disabled="submitting">
        {{ submitting ? 'Memproses…' : 'Masuk' }}
      </button>

      <p class="login__hint">
        Akun dikelola di area admin backend (<code>/admin/users</code>).
      </p>
    </form>
  </div>
</template>

<style scoped>
.login {
  max-width: 380px;
  padding: 1.3rem 1.4rem;
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.login__field {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.login__field span {
  font-size: 0.68rem;
  font-weight: 800;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--ink-muted);
}

.login__field input {
  border: 1px solid var(--line);
  border-radius: 10px;
  padding: 0.5rem 0.7rem;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--ink);
  font-family: inherit;
}

.login__field input:focus {
  outline: none;
  border-color: var(--accent-blue);
}

.login__submit {
  border: none;
  border-radius: 10px;
  background: var(--navy-bar, #1e2f83);
  color: #fff;
  padding: 0.55rem 1rem;
  font-size: 0.78rem;
  font-weight: 700;
  font-family: inherit;
}

.login__submit:disabled {
  opacity: 0.6;
}

.login__error {
  border-left: 3px solid #d93025;
  background: #fdeae8;
  color: #b3261e;
  border-radius: 8px;
  padding: 0.5rem 0.7rem;
  font-size: 0.74rem;
  font-weight: 700;
}

.login__hint {
  margin: 0;
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.login__hint code {
  background: #f1f3fa;
  border-radius: 5px;
  padding: 0.05rem 0.3rem;
}

.login__status {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  padding: 1.1rem 1.2rem;
  max-width: 520px;
}

.login__status-body {
  flex: 1;
  min-width: 0;
}

.login__status-title,
.login__status-role {
  margin: 0;
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--ink);
}

.login__status-role {
  margin-top: 0.15rem;
  font-size: 0.7rem;
  color: var(--ink-muted);
}

.login__logout {
  flex: none;
  border: 1px solid var(--line);
  border-radius: 9px;
  background: #fff;
  padding: 0.35rem 0.85rem;
  font-size: 0.74rem;
  font-weight: 700;
  color: var(--ink);
  font-family: inherit;
}

.login__logout:hover {
  border-color: #d93025;
  color: #b3261e;
}
</style>
