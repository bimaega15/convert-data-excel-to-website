<script setup>
import { computed, ref, watch } from 'vue'
import DashIcon from '../dashboard/DashIcon.vue'

const props = defineProps({
  // kolom: { key, label, width?, align?, sortable?=true, clamp?, nowrap? }
  columns: { type: Array, required: true },
  rows: { type: Array, required: true },
  initialSort: { type: Object, default: null }, // { key, dir: 'asc'|'desc' }
  pageSizeOptions: { type: Array, default: () => [10, 25, 50, 100] },
})

const q = ref('')
const sortKey = ref(props.initialSort?.key ?? null)
const sortDir = ref(props.initialSort?.dir ?? 'asc')
const page = ref(1)
const pageSize = ref(props.pageSizeOptions[0])

const filtered = computed(() => {
  const term = q.value.trim().toLowerCase()
  if (!term) return props.rows
  const keys = props.columns.map((c) => c.key)
  return props.rows.filter((row) =>
    keys.some((k) => {
      const v = row[k]
      return v != null && String(v).toLowerCase().includes(term)
    })
  )
})

const sorted = computed(() => {
  if (!sortKey.value) return filtered.value
  const dir = sortDir.value === 'asc' ? 1 : -1
  return [...filtered.value].sort((a, b) => {
    const va = a[sortKey.value]
    const vb = b[sortKey.value]
    if (va == null) return 1
    if (vb == null) return -1
    if (typeof va === 'number' && typeof vb === 'number') return (va - vb) * dir
    return String(va).localeCompare(String(vb), 'id', { numeric: true }) * dir
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(sorted.value.length / pageSize.value)))

const paged = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return sorted.value.slice(start, start + pageSize.value)
})

const rangeInfo = computed(() => {
  const total = sorted.value.length
  if (!total) return 'Tidak ada data'
  const start = (page.value - 1) * pageSize.value + 1
  const end = Math.min(page.value * pageSize.value, total)
  return `Menampilkan ${start}–${end} dari ${total} data`
})

const pageNumbers = computed(() => {
  const total = totalPages.value
  const current = page.value
  const windowSize = 5
  let start = Math.max(1, current - Math.floor(windowSize / 2))
  const end = Math.min(total, start + windowSize - 1)
  start = Math.max(1, end - windowSize + 1)
  return Array.from({ length: end - start + 1 }, (_, i) => start + i)
})

watch([q, pageSize], () => {
  page.value = 1
})

watch(totalPages, (t) => {
  if (page.value > t) page.value = t
})

function toggleSort(col) {
  if (col.sortable === false) return
  if (sortKey.value === col.key) {
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = col.key
    sortDir.value = 'asc'
  }
}
</script>

<template>
  <div class="panel dt">
    <div class="dt__toolbar">
      <label class="dt__search">
        <DashIcon name="search" :size="15" />
        <input v-model="q" type="search" placeholder="Cari di semua kolom…" />
      </label>
      <label class="dt__pagesize">
        <span>Baris</span>
        <select v-model.number="pageSize">
          <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
        </select>
      </label>
    </div>

    <div class="dt__scroll">
      <table class="dt__table">
        <thead>
          <tr>
            <th
              v-for="col in columns"
              :key="col.key"
              :style="{ width: col.width, textAlign: col.align ?? 'left' }"
              :class="{ 'dt__th--sortable': col.sortable !== false }"
              @click="toggleSort(col)"
            >
              <span class="dt__th-inner">
                {{ col.label }}
                <span v-if="sortKey === col.key" class="dt__sort">{{ sortDir === 'asc' ? '▲' : '▼' }}</span>
              </span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(row, ri) in paged" :key="ri">
            <td
              v-for="col in columns"
              :key="col.key"
              :style="{ textAlign: col.align ?? 'left' }"
              :class="{ 'dt__td--nowrap': col.nowrap }"
            >
              <slot :name="`cell-${col.key}`" :row="row" :value="row[col.key]">
                <span v-if="col.clamp" class="dt__clamp" :title="row[col.key] ?? ''">{{ row[col.key] ?? '-' }}</span>
                <template v-else>{{ row[col.key] ?? '-' }}</template>
              </slot>
            </td>
          </tr>
          <tr v-if="!paged.length">
            <td :colspan="columns.length" class="dt__empty">Tidak ada data yang cocok dengan pencarian.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="dt__footer">
      <span class="dt__info">{{ rangeInfo }}</span>
      <nav class="dt__pager" aria-label="Navigasi halaman">
        <button type="button" :disabled="page === 1" @click="page--">‹</button>
        <button
          v-for="p in pageNumbers"
          :key="p"
          type="button"
          :class="{ 'dt__page--active': p === page }"
          @click="page = p"
        >
          {{ p }}
        </button>
        <button type="button" :disabled="page === totalPages" @click="page++">›</button>
      </nav>
    </div>
  </div>
</template>

<style scoped>
.dt {
  overflow: visible;
}

.dt__toolbar {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.75rem 0.9rem;
  border-bottom: 1px solid var(--line);
}

.dt__search {
  flex: 1;
  min-width: 220px;
  max-width: 380px;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #f4f6fb;
  border: 1px solid var(--line);
  border-radius: 10px;
  padding: 0.4rem 0.7rem;
  color: var(--ink-muted);
}

.dt__search:focus-within {
  border-color: var(--accent-blue);
  background: #fff;
}

.dt__search input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 0.74rem;
  font-weight: 600;
  color: var(--ink);
  font-family: inherit;
}

.dt__pagesize {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 0.45rem;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink-muted);
}

.dt__pagesize select {
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 0.28rem 0.4rem;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--ink);
  background: #fff;
  font-family: inherit;
}

.dt__scroll {
  overflow-x: auto;
}

.dt__table {
  width: 100%;
  border-collapse: collapse;
  min-width: 760px;
}

.dt__table th {
  position: sticky;
  top: 0;
  background: #eef1f9;
  font-size: 0.62rem;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--ink);
  padding: 0.55rem 0.65rem;
  border-bottom: 2px solid var(--line);
  white-space: nowrap;
  user-select: none;
}

/* pemisah kolom: tabel master punya banyak kolom sehingga butuh grid penuh */
.dt__table th + th,
.dt__table td + td {
  border-left: 1px solid var(--line-soft);
}

.dt__th--sortable {
  cursor: pointer;
}

.dt__th--sortable:hover {
  color: var(--accent-blue);
}

.dt__sort {
  font-size: 0.55rem;
  margin-left: 0.15rem;
}

.dt__table td {
  padding: 0.5rem 0.65rem;
  border-bottom: 1px solid var(--line-soft);
  font-size: 0.7rem;
  font-weight: 500;
  color: var(--ink);
  vertical-align: top;
  line-height: 1.4;
}

.dt__table tbody tr:hover {
  background: #f6f8fd;
}

.dt__td--nowrap {
  white-space: nowrap;
}

.dt__clamp {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  max-width: 42ch;
}

.dt__empty {
  text-align: center;
  color: var(--ink-muted);
  padding: 1.5rem 0.65rem !important;
}

.dt__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.65rem 0.9rem;
}

.dt__info {
  font-size: 0.66rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.dt__pager {
  display: flex;
  gap: 0.25rem;
}

.dt__pager button {
  min-width: 30px;
  height: 30px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink);
  font-family: inherit;
}

.dt__pager button:hover:not(:disabled) {
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

.dt__pager button:disabled {
  opacity: 0.4;
}

.dt__page--active {
  background: var(--navy-bar) !important;
  border-color: var(--navy-bar) !important;
  color: #fff !important;
}
</style>
