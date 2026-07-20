<script setup>
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import PageHeader from '../components/ui/PageHeader.vue'
import DashIcon from '../components/dashboard/DashIcon.vue'
import { sheetBySlug } from '../data/sheets'

const route = useRoute()

// Data mentah tiap sheet di-load lazy: hanya file sheet yang dibuka yang diambil,
// sehingga bundle awal tetap ringan meski workbook punya puluhan sheet.
const sheetModules = import.meta.glob('../data/generated/sheets/*.json')

const meta = ref(null)
const data = ref(null) // { name, slug, rowCount, colCount, rows }
const loading = ref(false)
const notFound = ref(false)

const PAGE_SIZES = [25, 50, 100, 500]
const pageSize = ref(PAGE_SIZES[0])
const page = ref(1)
const q = ref('')

async function load(slug) {
  meta.value = sheetBySlug[slug] ?? null
  data.value = null
  notFound.value = false
  page.value = 1
  q.value = ''

  const loader = sheetModules[`../data/generated/sheets/${slug}.json`]
  if (!loader) {
    notFound.value = true
    return
  }

  loading.value = true
  try {
    const mod = await loader()
    data.value = mod.default ?? mod
  } finally {
    loading.value = false
  }
}

watch(() => route.params.slug, (slug) => slug && load(slug), { immediate: true })

// Baris pertama diperlakukan sebagai judul kolom, sisanya data (sama seperti
// preview di halaman Import).
const headerRow = computed(() => data.value?.rows[0] ?? [])
const bodyRows = computed(() => data.value?.rows.slice(1) ?? [])

const filtered = computed(() => {
  const term = q.value.trim().toLowerCase()
  if (!term) return bodyRows.value
  return bodyRows.value.filter((cells) =>
    cells.some((v) => String(v).toLowerCase().includes(term))
  )
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / pageSize.value)))

const paged = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filtered.value.slice(start, start + pageSize.value)
})

const rangeInfo = computed(() => {
  const total = filtered.value.length
  if (!total) return q.value ? 'Tidak ada baris yang cocok dengan pencarian.' : 'Sheet ini tidak memiliki baris data.'
  const start = (page.value - 1) * pageSize.value + 1
  const end = Math.min(page.value * pageSize.value, total)
  return `Menampilkan ${start}–${end} dari ${total} baris data`
})

const pageNumbers = computed(() => {
  const total = totalPages.value
  const windowSize = 5
  let start = Math.max(1, page.value - Math.floor(windowSize / 2))
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

function colLetter(i) {
  let s = ''
  let n = i
  do {
    s = String.fromCharCode(65 + (n % 26)) + s
    n = Math.floor(n / 26) - 1
  } while (n >= 0)
  return s
}
</script>

<template>
  <div class="master-page">
    <PageHeader
      :title="meta?.label ?? data?.name ?? 'Worksheet'"
      subtitle="Tampilan hanya-baca isi worksheet apa adanya dari file Excel. Untuk mengubah data, gunakan halaman Import Excel."
    >
      <template #right>
        <span v-if="meta" class="stat-chip">Sheet <strong>{{ meta.name }}</strong></span>
        <span v-if="data" class="stat-chip">Baris <strong>{{ Math.max(0, data.rowCount - 1) }}</strong></span>
        <span v-if="data" class="stat-chip">Kolom <strong>{{ data.colCount }}</strong></span>
      </template>
    </PageHeader>

    <div v-if="notFound" class="panel sv__state">
      <DashIcon name="warning" :size="22" />
      <p>Worksheet <strong>{{ route.params.slug }}</strong> tidak ditemukan di data hasil konversi.</p>
    </div>

    <div v-else-if="loading" class="panel sv__state">Memuat data worksheet…</div>

    <div v-else-if="data && data.rowCount === 0" class="panel sv__state">
      Sheet ini kosong — tidak ada sel berisi data.
    </div>

    <div v-else-if="data" class="panel sv">
      <div class="sv__toolbar">
        <label class="sv__search">
          <DashIcon name="search" :size="15" />
          <input v-model="q" type="search" placeholder="Cari di semua kolom…" />
        </label>
        <label class="sv__pagesize">
          <span>Baris</span>
          <select v-model.number="pageSize">
            <option v-for="s in PAGE_SIZES" :key="s" :value="s">{{ s }}</option>
          </select>
        </label>
      </div>

      <div class="sv__scroll">
        <table class="sv__table">
          <thead>
            <tr class="sv__collabels">
              <th class="sv__gutter sv__gutter--head" rowspan="2" title="Nomor baris data">#</th>
              <th v-for="(_, i) in headerRow" :key="`c${i}`">{{ colLetter(i) }}</th>
            </tr>
            <tr>
              <th
                v-for="(cell, i) in headerRow"
                :key="`h${i}`"
                :title="String(cell)"
              >
                {{ cell === '' ? '—' : cell }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(cells, ri) in paged" :key="ri">
              <td class="sv__gutter">{{ (page - 1) * pageSize + ri + 1 }}</td>
              <td v-for="(cell, i) in cells" :key="i" :title="String(cell)">
                <span class="sv__cell">{{ cell }}</span>
              </td>
            </tr>
            <tr v-if="!paged.length">
              <td :colspan="headerRow.length + 1" class="sv__empty">
                Tidak ada baris yang cocok dengan pencarian.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="sv__footer">
        <span class="sv__info">{{ rangeInfo }}</span>
        <nav v-if="totalPages > 1" class="sv__pager" aria-label="Navigasi halaman">
          <button type="button" :disabled="page === 1" @click="page--">‹</button>
          <button
            v-for="p in pageNumbers"
            :key="p"
            type="button"
            :class="{ 'sv__page--active': p === page }"
            @click="page = p"
          >
            {{ p }}
          </button>
          <button type="button" :disabled="page === totalPages" @click="page++">›</button>
        </nav>
      </div>
    </div>
  </div>
</template>

<style scoped>
.sv {
  overflow: hidden;
}

.sv__state {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 1.5rem 1.1rem;
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.sv__state strong {
  color: var(--ink-strong);
}

.sv__toolbar {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.75rem 0.9rem;
  border-bottom: 1px solid var(--line);
}

.sv__search {
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

.sv__search:focus-within {
  border-color: var(--accent-blue);
  background: #fff;
}

.sv__search input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 0.74rem;
  font-weight: 600;
  color: var(--ink);
  font-family: inherit;
}

.sv__pagesize {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 0.45rem;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink-muted);
}

.sv__pagesize select {
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 0.28rem 0.4rem;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--ink);
  background: #fff;
  font-family: inherit;
}

.sv__scroll {
  overflow: auto;
  max-height: 66vh;
}

.sv__table {
  border-collapse: collapse;
  font-size: 0.68rem;
  width: 100%;
}

.sv__table th,
.sv__table td {
  border-right: 1px solid var(--line-soft);
  border-bottom: 1px solid var(--line-soft);
  padding: 0.32rem 0.5rem;
  text-align: left;
  vertical-align: top;
}

.sv__table thead th {
  position: sticky;
  background: #eef1f9;
  font-weight: 800;
  color: var(--ink);
  white-space: nowrap;
  z-index: 2;
}

.sv__collabels th {
  top: 0;
  font-size: 0.6rem;
  color: var(--ink-muted);
  text-align: center;
  background: #e4e9f4;
}

.sv__table thead tr:nth-child(2) th {
  top: 22px;
  border-bottom: 2px solid var(--line);
  max-width: 26ch;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sv__gutter {
  position: sticky;
  left: 0;
  z-index: 3;
  background: #eef1f9 !important;
  color: var(--ink-muted) !important;
  font-weight: 700;
  text-align: center !important;
  font-variant-numeric: tabular-nums;
  min-width: 42px;
}

.sv__table thead th.sv__gutter--head {
  top: 0;
  z-index: 5;
  vertical-align: middle;
  background: #e4e9f4;
  border-bottom: 2px solid var(--line);
}

.sv__table tbody tr:hover td {
  background: #f6f8fd;
}

.sv__cell {
  display: block;
  max-width: 38ch;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sv__empty {
  text-align: center;
  color: var(--ink-muted);
  padding: 1.5rem 0.65rem !important;
}

.sv__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.65rem 0.9rem;
  border-top: 1px solid var(--line);
}

.sv__info {
  font-size: 0.68rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.sv__pager {
  display: flex;
  gap: 0.25rem;
}

.sv__pager button {
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

.sv__pager button:hover:not(:disabled) {
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

.sv__pager button:disabled {
  opacity: 0.4;
}

.sv__page--active {
  background: var(--navy-bar) !important;
  border-color: var(--navy-bar) !important;
  color: #fff !important;
}
</style>
