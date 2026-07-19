<script setup>
defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
})
</script>

<template>
  <div class="page-header">
    <div>
      <h2>{{ title }}</h2>
      <p v-if="subtitle">{{ subtitle }}</p>
    </div>
    <div class="page-header__right">
      <slot name="right"></slot>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 0.9rem;
}

.page-header h2 {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  color: var(--ink-strong);
}

.page-header p {
  margin: 0.15rem 0 0;
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--ink-muted);
  max-width: 70ch;
}

.page-header__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.page-header__right :deep(.stat-chip) {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  background: var(--surface);
  border: 1px solid var(--line);
  border-radius: 999px;
  padding: 0.32rem 0.8rem;
  font-size: 0.66rem;
  font-weight: 700;
  color: var(--ink);
}

.page-header__right :deep(.stat-chip strong) {
  color: var(--accent-blue);
  font-weight: 800;
}

.page-header__right :deep(.stat-chip--edit) {
  background: #fff6dd;
  border-color: #e9d59a;
  color: #7a5800;
}

.page-header__right :deep(.stat-chip--edit strong) {
  color: #7a5800;
}
</style>
