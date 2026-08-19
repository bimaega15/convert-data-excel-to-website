<script setup>
import { ref, reactive, watch } from 'vue'
import DashIcon from '../dashboard/DashIcon.vue'

const props = defineProps({
  show: { type: Boolean, default: false },
  title: { type: String, default: 'Tambah Baris Baru' },
  columns: { type: Array, required: true },
  submitting: { type: Boolean, default: false },
  errorText: { type: String, default: '' }
})

const emit = defineEmits(['close', 'submit'])

const form = reactive({})

watch(() => props.show, (val) => {
  if (val) {
    // Reset form fields
    props.columns.forEach(col => {
      if (col.key !== 'key' && col.key !== 'id') {
        form[col.key] = col.type === 'number' ? 0 : ''
      }
    })
  }
})

function onSubmit() {
  emit('submit', { ...form })
}
</script>

<template>
  <div v-if="show" class="modal-backdrop" @click.self="emit('close')">
    <div class="modal-card">
      <div class="modal-header">
        <h3 class="modal-title">{{ title }}</h3>
        <button type="button" class="btn-close" @click="emit('close')">
          <DashIcon name="close" :size="16" />
        </button>
      </div>

      <form @submit.prevent="onSubmit" class="modal-body">
        <div v-if="errorText" class="modal-error">
          <DashIcon name="warning" :size="16" /> {{ errorText }}
        </div>

        <div class="form-grid">
          <div
            v-for="col in columns.filter(c => c.key !== 'key' && c.key !== 'id')"
            :key="col.key"
            class="form-group"
          >
            <label :for="`input-${col.key}`" class="form-label">{{ col.label }}</label>
            <input
              v-if="col.type === 'number'"
              :id="`input-${col.key}`"
              v-model.number="form[col.key]"
              type="number"
              class="form-input"
              :placeholder="`Masukkan ${col.label.toLowerCase()}`"
            />
            <textarea
              v-else-if="col.clamp || col.type === 'textarea'"
              :id="`input-${col.key}`"
              v-model="form[col.key]"
              class="form-input form-textarea"
              rows="2"
              :placeholder="`Masukkan ${col.label.toLowerCase()}`"
            ></textarea>
            <input
              v-else
              :id="`input-${col.key}`"
              v-model="form[col.key]"
              type="text"
              class="form-input"
              :placeholder="`Masukkan ${col.label.toLowerCase()}`"
            />
          </div>
        </div>

        <div class="modal-footer">
          <button type="button" class="btn btn-ghost" :disabled="submitting" @click="emit('close')">Batal</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <DashIcon v-if="!submitting" name="plus" :size="14" />
            <span>{{ submitting ? 'Menyimpan…' : 'Simpan Baris' }}</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  backdrop-filter: blur(4px);
  z-index: 999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
}

.modal-card {
  background: #ffffff;
  border-radius: 16px;
  width: 100%;
  max-width: 600px;
  max-height: 85vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 40px -15px rgba(0, 0, 0, 0.2);
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.1rem 1.4rem;
  border-bottom: 1px solid var(--line);
  background: #f8fafc;
}

.modal-title {
  font-size: 1rem;
  font-weight: 800;
  color: var(--ink);
  margin: 0;
}

.btn-close {
  background: transparent;
  border: none;
  color: var(--ink-muted);
  cursor: pointer;
  padding: 0.3rem;
  border-radius: 6px;
}

.btn-close:hover {
  background: #e2e8f0;
  color: var(--ink);
}

.modal-body {
  padding: 1.2rem 1.4rem;
  overflow-y: auto;
  flex: 1;
}

.modal-error {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 0.8rem;
  background: #fdeae8;
  color: #b3261e;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 700;
  margin-bottom: 1rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.form-label {
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--ink);
}

.form-input {
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 0.5rem 0.7rem;
  font-size: 0.78rem;
  font-family: inherit;
  color: var(--ink);
  background: #fff;
}

.form-input:focus {
  border-color: var(--accent-blue);
  outline: none;
}

.form-textarea {
  resize: vertical;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.6rem;
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--line);
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  border-radius: 8px;
  padding: 0.5rem 1rem;
  font-size: 0.78rem;
  font-weight: 800;
  cursor: pointer;
  border: 1px solid transparent;
}

.btn-primary {
  background: var(--navy-bar);
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  opacity: 0.9;
}

.btn-ghost {
  background: #fff;
  border-color: var(--line);
  color: var(--ink);
}

.btn:disabled {
  opacity: 0.6;
  cursor: default;
}
</style>
