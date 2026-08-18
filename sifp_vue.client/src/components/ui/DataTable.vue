<script setup>
import { computed, ref, watch } from 'vue'
import DashIcon from '../dashboard/DashIcon.vue'

const props = defineProps({
  // kolom: { key, label, width?, align?, sortable?=true, clamp?, nowrap? }
  columns: { type: Array, required: true },
  rows: { type: Array, required: true },
  initialSort: { type: Object, default: null }, // { key, dir: 'asc'|'desc' }
  pageSizeOptions: { type: Array, default: () => [10, 25, 50, 100] },
  // Aktifkan kolom checkbox + aksi hapus massal. rowKey = properti unik tiap baris.
  selectable: { type: Boolean, default: false },
  rowKey: { type: String, default: 'id' },
  // Diset true oleh induk selama proses hapus berjalan (menonaktifkan tombol).
  deleting: { type: Boolean, default: false },
  // Pesan error hapus (opsional) yang ditampilkan sebagai banner di atas tabel.
  errorText: { type: String, default: '' },
})

const emit = defineEmits(['delete'])

const selectedKeys = ref(new Set())
const confirming = ref(false)

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

/* ===== seleksi & hapus massal ===== */

// Baris yang bisa dipilih = hasil pencarian saat ini (yang terlihat user).
// Tanpa pencarian, ini berarti seluruh baris -> "pilih semua" = semua data.
const selectableKeys = computed(() => filtered.value.map((r) => r[props.rowKey]))
const selectedCount = computed(() => selectedKeys.value.size)
const allSelected = computed(
  () => selectableKeys.value.length > 0 && selectableKeys.value.every((k) => selectedKeys.value.has(k))
)
const someSelected = computed(
  () => !allSelected.value && selectableKeys.value.some((k) => selectedKeys.value.has(k))
)

const isSelected = (key) => selectedKeys.value.has(key)

function toggleRow(key) {
  const next = new Set(selectedKeys.value)
  if (next.has(key)) next.delete(key)
  else next.add(key)
  selectedKeys.value = next
}

function toggleAll() {
  const next = new Set(selectedKeys.value)
  if (allSelected.value) selectableKeys.value.forEach((k) => next.delete(k))
  else selectableKeys.value.forEach((k) => next.add(k))
  selectedKeys.value = next
}

function clearSelection() {
  selectedKeys.value = new Set()
  confirming.value = false
}

function confirmDelete() {
  emit('delete', Array.from(selectedKeys.value))
}

// Data dimuat ulang (mis. setelah hapus berhasil) -> reset seleksi & konfirmasi.
watch(() => props.rows, clearSelection)
</script>

<template>
  <div class="panel dt">
    <div v-if="errorText" class="dt__error" role="alert">
      <DashIcon name="warning" :size="15" /> {{ errorText }}
    </div>

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

    <!-- bar aksi hapus massal: muncul saat ada baris terpilih -->
    <div v-if="selectable && selectedCount" class="dt__bulk">
      <template v-if="!confirming">
        <span class="dt__bulk-info"><strong>{{ selectedCount }}</strong> baris dipilih</span>
        <div class="dt__bulk-actions">
          <button type="button" class="dt__btn dt__btn--danger" @click="confirming = true">
            <DashIcon name="trash" :size="14" /> Hapus terpilih
          </button>
          <button type="button" class="dt__btn dt__btn--ghost" @click="clearSelection">Batal pilih</button>
        </div>
      </template>
      <template v-else>
        <span class="dt__bulk-info dt__bulk-info--warn">
          <DashIcon name="warning" :size="14" /> Hapus <strong>{{ selectedCount }}</strong> baris? Tindakan ini permanen.
        </span>
        <div class="dt__bulk-actions">
          <button type="button" class="dt__btn dt__btn--danger" :disabled="deleting" @click="confirmDelete">
            {{ deleting ? 'Menghapus…' : 'Ya, hapus' }}
          </button>
          <button type="button" class="dt__btn dt__btn--ghost" :disabled="deleting" @click="confirming = false">
            Batal
          </button>
        </div>
      </template>
    </div>

    <div class="dt__scroll">
      <table class="dt__table">
        <thead>
          <tr>
            <th v-if="selectable" class="dt__check-col">
              <input
                type="checkbox"
                :checked="allSelected"
                :indeterminate.prop="someSelected"
                :aria-label="allSelected ? 'Batalkan pilih semua' : 'Pilih semua'"
                @change="toggleAll"
              />
            </th>
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
          <tr
            v-for="(row, ri) in paged"
            :key="ri"
            :class="{ 'dt__row--selected': selectable && isSelected(row[rowKey]) }"
          >
            <td v-if="selectable" class="dt__check-col">
              <input
                type="checkbox"
                :checked="isSelected(row[rowKey])"
                :aria-label="`Pilih baris ${(page - 1) * pageSize + ri + 1}`"
                @change="toggleRow(row[rowKey])"
              />
            </td>
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
            <td :colspan="columns.length + (selectable ? 1 : 0)" class="dt__empty">
              Tidak ada data yang cocok dengan pencarian.
            </td>
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

.dt__error {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0.55rem 0.9rem;
  background: #fdeae8;
  border-bottom: 1px solid #f0bdb8;
  color: #b3261e;
  font-size: 0.72rem;
  font-weight: 700;
}

/* ===== bar aksi hapus massal ===== */
.dt__bulk {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.55rem 0.9rem;
  background: #fdefea;
  border-bottom: 1px solid #f3c9bd;
}

.dt__bulk-info {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--ink);
}

.dt__bulk-info strong {
  color: var(--accent-red);
}

.dt__bulk-info--warn {
  color: #b3261e;
}

.dt__bulk-actions {
  display: flex;
  gap: 0.5rem;
}

.dt__btn {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  border-radius: 8px;
  padding: 0.4rem 0.85rem;
  font-family: inherit;
  font-size: 0.72rem;
  font-weight: 800;
  cursor: pointer;
  border: 1px solid transparent;
}

.dt__btn:disabled {
  opacity: 0.6;
  cursor: default;
}

.dt__btn--danger {
  background: var(--accent-red);
  color: #fff;
}

.dt__btn--danger:hover:not(:disabled) {
  background: #a01b12;
}

.dt__btn--ghost {
  background: #fff;
  border-color: var(--line);
  color: var(--ink);
}

.dt__btn--ghost:hover:not(:disabled) {
  border-color: var(--ink-muted);
}

/* ===== kolom checkbox ===== */
.dt__check-col {
  width: 42px;
  text-align: center !important;
  padding-left: 0.4rem !important;
  padding-right: 0.4rem !important;
}

.dt__check-col input {
  width: 15px;
  height: 15px;
  accent-color: var(--navy-bar);
  cursor: pointer;
  vertical-align: middle;
}

.dt__table tbody tr.dt__row--selected {
  background: #eaf1ff;
}

.dt__table tbody tr.dt__row--selected:hover {
  background: #e0eaff;
}

.dt__scroll {
  overflow-x: auto;
  /* menahan scroll horizontal tetap di dalam panel, bukan meluber ke halaman */
  max-width: 100%;
}

.dt__table {
  /* melebar mengikuti konten kolom (tidak menyempit), minimal selebar panel;
     kombinasi ini memunculkan scroll horizontal di .dt__scroll saat kolom banyak */
  width: max-content;
  min-width: 100%;
  border-collapse: collapse;
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
