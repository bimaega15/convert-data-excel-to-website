<script setup>
import { computed, nextTick, ref, watch } from 'vue'

const props = defineProps({
  sheet: { type: Object, required: true },
  // { "excelRow:col": { excelRow, col, from, to } } khusus sheet yang sedang dibuka
  edits: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['edit', 'revert'])

const PAGE_SIZES = [25, 50, 100, 500]
const pageSize = ref(PAGE_SIZES[0])
const page = ref(1)

// Baris berisi data yang pertama diperlakukan sebagai judul kolom, sisanya data.
const headerRow = computed(() => props.sheet.rows[0] ?? null)
const bodyRows = computed(() => props.sheet.rows.slice(1))

const totalPages = computed(() => Math.max(1, Math.ceil(bodyRows.value.length / pageSize.value)))

// Ditulis lengkap supaya selisih "11 baris" vs "10 baris data" tidak terbaca
// sebagai salah hitung — baris pertama adalah judul kolom, bukan data.
const rowSummary = computed(
  () =>
    `${props.sheet.rowCount} baris (1 judul + ${bodyRows.value.length} data) × ${props.sheet.colCount} kolom`
)

const pagedRows = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return bodyRows.value.slice(start, start + pageSize.value)
})

// Urutan navigasi keyboard: baris judul lalu baris data yang sedang tampil.
const navRows = computed(() => (headerRow.value ? [headerRow.value, ...pagedRows.value] : []))

// Footer menghitung baris DATA (ordinal 1..N) untuk navigasi halaman, sedangkan
// kolom nomor di kiri tabel memakai nomor baris Excel asli (baris judul = 1)
// supaya sel bisa dicocokkan langsung dengan file & daftar perubahan.
const rangeInfo = computed(() => {
  const total = bodyRows.value.length
  if (!total) return 'Sheet ini hanya berisi baris judul, tanpa baris data.'
  const start = (page.value - 1) * pageSize.value + 1
  const end = Math.min(start + pagedRows.value.length - 1, total)
  if (totalPages.value === 1) {
    return `Menampilkan seluruh ${total} baris data`
  }
  return `Menampilkan baris ${start}–${end} dari ${total} baris data`
})

/* ===== editing sel ===== */

const editing = ref(null) // { excelRow, col }
const draft = ref('')
const inputEl = ref(null)

// ref biasa di dalam v-for dikumpulkan Vue menjadi array; input edit selalu
// tunggal, jadi dipasang lewat function ref agar bisa langsung difokuskan.
const setInputRef = (el) => {
  if (el) inputEl.value = el
}

// blur ikut terpicu saat input dipindah antar sel oleh Tab/Enter — tanpa
// penanda ini, commit dari blur akan membatalkan sel yang baru saja dibuka.
let movingFocus = false

const keyOf = (excelRow, col) => `${excelRow}:${col}`

function editOf(excelRow, col) {
  return props.edits[keyOf(excelRow, col)]
}

/** Nilai yang ditampilkan: hasil edit bila ada, kalau tidak nilai asli file. */
function cellValue(row, col) {
  const e = editOf(row.excelRow, col)
  return e ? e.to : row.cells[col]
}

function isEditing(row, col) {
  return editing.value?.excelRow === row.excelRow && editing.value?.col === col
}

function cellTitle(row, col) {
  const e = editOf(row.excelRow, col)
  const shown = cellValue(row, col)
  if (e) return `Diubah — semula: ${e.from === '' ? '(kosong)' : e.from}`
  return shown
}

async function startEdit(row, col) {
  editing.value = { excelRow: row.excelRow, col }
  draft.value = cellValue(row, col)
  await nextTick()
  inputEl.value?.focus()
  inputEl.value?.select()
}

function commit() {
  if (!editing.value) return
  const { excelRow, col } = editing.value
  const row = props.sheet.rows.find((r) => r.excelRow === excelRow)
  editing.value = null
  if (!row) return

  const original = row.cells[col]
  const next = draft.value
  // kembali ke nilai asli = perubahan dihapus, bukan dicatat sebagai edit
  if (next === original) emit('revert', { excelRow, col })
  else emit('edit', { excelRow, col, from: original, to: next })
}

function onBlur() {
  if (!movingFocus) commit()
}

function cancel() {
  editing.value = null
}

/** Commit lalu pindah ke sel berikutnya (Tab) atau baris berikutnya (Enter). */
async function commitAndMove(dCol, dRow) {
  const current = editing.value
  if (!current) return

  movingFocus = true
  try {
    commit()

    const rowIdx = navRows.value.findIndex((r) => r.excelRow === current.excelRow)
    if (rowIdx < 0) return

    let col = current.col + dCol
    let idx = rowIdx + dRow

    if (col >= props.sheet.colCount) {
      col = 0
      idx += 1
    } else if (col < 0) {
      col = props.sheet.colCount - 1
      idx -= 1
    }

    const target = navRows.value[idx]
    if (target) await startEdit(target, col)
  } finally {
    movingFocus = false
  }
}

// pindah sheet / ganti halaman: batalkan editing yang sedang berjalan
watch(
  () => [props.sheet.name, page.value, pageSize.value],
  () => {
    editing.value = null
  }
)

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
        <span title="Baris kosong tidak ikut dihitung; nomor baris tetap mengikuti file Excel">
          {{ rowSummary }}
        </span>
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
      <p class="sp__hint">
        Klik sel mana pun untuk mengubah isinya. <kbd>Enter</kbd> simpan · <kbd>Tab</kbd> sel
        berikutnya · <kbd>Esc</kbd> batal.
      </p>

      <div class="sp__scroll">
        <table class="sp__table">
          <thead>
            <tr class="sp__collabels">
              <th class="sp__gutter sp__gutter--head" title="Nomor baris sesuai file Excel">#</th>
              <th v-for="(_, i) in headerRow.cells" :key="`c${i}`">{{ colLetter(i) }}</th>
            </tr>
            <tr>
              <!-- baris judul = baris Excel pertama; diberi nomor barisnya juga supaya
                   konsisten dengan nomor baris data dan referensi sel di daftar perubahan -->
              <th
                class="sp__gutter sp__gutter--headrow"
                :title="`Baris Excel: ${headerRow.excelRow}`"
              >
                {{ headerRow.excelRow }}
              </th>
              <th
                v-for="(_, i) in headerRow.cells"
                :key="`h${i}`"
                class="sp__editable"
                :class="{ 'is-edited': !!editOf(headerRow.excelRow, i) }"
                :title="cellTitle(headerRow, i)"
                @click="startEdit(headerRow, i)"
              >
                <input
                  v-if="isEditing(headerRow, i)"
                  :ref="setInputRef"
                  v-model="draft"
                  class="sp__input"
                  @keydown.enter.prevent="commitAndMove(0, 1)"
                  @keydown.tab.prevent="commitAndMove(1, 0)"
                  @keydown.shift.tab.prevent="commitAndMove(-1, 0)"
                  @keydown.esc.prevent="cancel"
                  @blur="onBlur"
                />
                <template v-else>{{
                  cellValue(headerRow, i) === '' ? '—' : cellValue(headerRow, i)
                }}</template>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, rowIdx) in pagedRows" :key="row.excelRow">
              <td class="sp__gutter" :title="`Baris data ke-${(page - 1) * pageSize + rowIdx + 1}`">{{ row.excelRow }}</td>
              <td
                v-for="(_, i) in row.cells"
                :key="i"
                class="sp__editable"
                :class="{ 'is-edited': !!editOf(row.excelRow, i) }"
                :title="cellTitle(row, i)"
                @click="startEdit(row, i)"
              >
                <input
                  v-if="isEditing(row, i)"
                  :ref="setInputRef"
                  v-model="draft"
                  class="sp__input"
                  @keydown.enter.prevent="commitAndMove(0, 1)"
                  @keydown.tab.prevent="commitAndMove(1, 0)"
                  @keydown.shift.tab.prevent="commitAndMove(-1, 0)"
                  @keydown.esc.prevent="cancel"
                  @blur="onBlur"
                />
                <span v-else class="sp__cell">{{ cellValue(row, i) }}</span>
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

.sp__hint {
  margin: 0;
  padding: 0.45rem 0.9rem;
  font-size: 0.66rem;
  font-weight: 600;
  color: var(--ink-muted);
  background: #f8f9fd;
  border-bottom: 1px solid var(--line-soft);
}

.sp__hint kbd {
  background: #fff;
  border: 1px solid var(--line);
  border-bottom-width: 2px;
  border-radius: 5px;
  padding: 0.02rem 0.28rem;
  font-family: inherit;
  font-size: 0.92em;
  font-weight: 800;
  color: var(--ink);
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
  max-height: 58vh;
}

.sp__table {
  border-collapse: collapse;
  font-size: 0.68rem;
  width: 100%;
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
  text-align: center !important;
  font-variant-numeric: tabular-nums;
  min-width: 42px;
}

/* Sel "#" membentang di kedua baris header dan menempel di dua arah sekaligus,
   jadi harus tergambar di atas nomor baris tbody yang juga sticky-left.
   Selector disamakan spesifisitasnya dengan ".sp__table thead th" agar
   z-index-nya tidak dikalahkan aturan header umum. */
.sp__table thead th.sp__gutter--head {
  top: 0;
  z-index: 5;
  vertical-align: middle;
  text-align: center;
  background: #e4e9f4;
  border-bottom: 2px solid var(--line);
}

/* nomor baris untuk baris judul (baris Excel pertama), menempel di bawah label kolom */
.sp__table thead th.sp__gutter--headrow {
  top: 22px;
  z-index: 5;
}

.sp__table tbody tr:hover td {
  background: #f6f8fd;
}

.sp__editable {
  cursor: text;
  position: relative;
}

.sp__editable:hover {
  outline: 1px solid var(--line);
  outline-offset: -1px;
}

/* sel yang diubah ditandai agar mudah ditemukan kembali sebelum submit */
.sp__editable.is-edited {
  background: #fff6dd !important;
  box-shadow: inset 3px 0 0 var(--st-degraded);
}

.sp__cell {
  display: block;
  max-width: 32ch;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sp__input {
  width: 100%;
  min-width: 12ch;
  border: 2px solid var(--accent-blue);
  border-radius: 5px;
  padding: 0.12rem 0.3rem;
  font-family: inherit;
  font-size: 0.68rem;
  font-weight: 600;
  color: var(--ink-strong);
  background: #fff;
  outline: none;
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
