<script setup>
import { computed, ref, watch } from 'vue'

const props = defineProps({
  sheet: { type: Object, required: true },
})

const PAGE_SIZES = [25, 50, 100, 500]
const pageSize = ref(PAGE_SIZES[0])
const page = ref(1)

// Baris pertama diperlakukan sebagai judul kolom, sisanya data.
const header = computed(() => props.sheet.grid[0] ?? [])
const bodyRows = computed(() => props.sheet.grid.slice(1))

const totalPages = computed(() => Math.max(1, Math.ceil(bodyRows.value.length / pageSize.value)))

const pagedRows = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return bodyRows.value.slice(start, start + pageSize.value).map((cells, i) => ({
    // nomor baris mengikuti penomoran Excel (baris 1 = judul kolom)
    excelRow: start + i + 2,
    cells,
  }))
})

const rangeInfo = computed(() => {
  const total = bodyRows.value.length
  if (!total) return 'Sheet ini tidak memiliki baris data.'
  const start = (page.value - 1) * pageSize.value + 1
  const end = Math.min(page.value * pageSize.value, total)
  return `Baris data ${start}–${end} dari ${total}`
})

// reset paginasi saat berpindah sheet atau mengubah jumlah baris
watch(() => props.sheet.name, () => {
  page.value = 1
})
watch(pageSize, () => {
  page.value = 1
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
  <div class="sp">
    <div class="sp__bar">
      <div class="sp__id">
        <h3>{{ sheet.name }}</h3>
        <span class="chip" :class="sheet.required ? 'chip--req' : 'chip--extra'">
          {{ sheet.required ? 'Dipakai converter' : 'Sheet tambahan' }}
        </span>
      </div>
      <div class="sp__meta">
        <span>{{ sheet.rowCount }} baris × {{ sheet.colCount }} kolom</span>
        <label>
          Tampilkan
          <select v-model.number="pageSize">
            <option v-for="s in PAGE_SIZES" :key="s" :value="s">{{ s }}</option>
          </select>
        </label>
      </div>
    </div>

    <div v-if="sheet.empty" class="sp__empty">Sheet ini kosong — tidak ada sel berisi data.</div>

    <template v-else>
      <div class="sp__scroll">
        <table class="sp__table">
          <thead>
            <tr class="sp__collabels">
              <th class="sp__gutter"></th>
              <th v-for="(_, i) in header" :key="`c${i}`">{{ colLetter(i) }}</th>
            </tr>
            <tr>
              <th class="sp__gutter">1</th>
              <th v-for="(cell, i) in header" :key="`h${i}`" :title="cell">
                {{ cell === '' ? '—' : cell }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in pagedRows" :key="row.excelRow">
              <td class="sp__gutter">{{ row.excelRow }}</td>
              <td v-for="(cell, i) in row.cells" :key="i" :title="cell">
                <span class="sp__cell">{{ cell }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="sp__footer">
        <span class="sp__info">{{ rangeInfo }}</span>
        <nav v-if="totalPages > 1" class="sp__pager">
          <button type="button" :disabled="page === 1" @click="page--">‹</button>
          <span>Hal. {{ page }} / {{ totalPages }}</span>
          <button type="button" :disabled="page === totalPages" @click="page++">›</button>
        </nav>
      </div>
    </template>
  </div>
</template>

<style scoped>
.sp {
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.sp__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.7rem 0.9rem;
  border-bottom: 1px solid var(--line);
}

.sp__id {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 0;
}

.sp__id h3 {
  margin: 0;
  font-size: 0.85rem;
  font-weight: 800;
  color: var(--ink-strong);
  word-break: break-all;
}

.chip--req {
  background: #e2f3e9;
  color: #0a6b38;
}

.chip--extra {
  background: #eef0f4;
  color: #5b6472;
}

.sp__meta {
  display: flex;
  align-items: center;
  gap: 0.9rem;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink-muted);
  white-space: nowrap;
}

.sp__meta select {
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 0.22rem 0.35rem;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink);
  background: #fff;
  font-family: inherit;
  margin-left: 0.3rem;
}

.sp__empty {
  padding: 2.5rem 1rem;
  text-align: center;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.sp__scroll {
  overflow: auto;
  max-height: 62vh;
}

.sp__table {
  border-collapse: collapse;
  font-size: 0.68rem;
}

.sp__table th,
.sp__table td {
  border-right: 1px solid var(--line-soft);
  border-bottom: 1px solid var(--line-soft);
  padding: 0.32rem 0.5rem;
  text-align: left;
  vertical-align: top;
}

.sp__table thead th {
  position: sticky;
  background: #eef1f9;
  font-weight: 800;
  color: var(--ink);
  white-space: nowrap;
  z-index: 2;
}

.sp__collabels th {
  top: 0;
  font-size: 0.6rem;
  color: var(--ink-muted);
  text-align: center;
  background: #e4e9f4;
}

.sp__table thead tr:nth-child(2) th {
  top: 22px;
  border-bottom: 2px solid var(--line);
  max-width: 26ch;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* kolom nomor baris ikut menempel saat digulir horizontal */
.sp__gutter {
  position: sticky;
  left: 0;
  z-index: 3;
  background: #eef1f9 !important;
  color: var(--ink-muted) !important;
  font-weight: 700;
  text-align: right !important;
  font-variant-numeric: tabular-nums;
  min-width: 42px;
}

.sp__table tbody tr:hover td {
  background: #f6f8fd;
}

.sp__cell {
  display: block;
  max-width: 32ch;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sp__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  flex-wrap: wrap;
  padding: 0.6rem 0.9rem;
  border-top: 1px solid var(--line);
}

.sp__info {
  font-size: 0.68rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.sp__pager {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--ink);
}

.sp__pager button {
  min-width: 28px;
  height: 28px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--ink);
  font-family: inherit;
}

.sp__pager button:disabled {
  opacity: 0.4;
}
</style>
