<script setup>
import { computed, ref, shallowRef } from 'vue'
import PageHeader from '../components/ui/PageHeader.vue'
import SheetPreview from '../components/import/SheetPreview.vue'
import DashIcon from '../components/dashboard/DashIcon.vue'
import {
  EndpointNotConfiguredError,
  IMPORT_ENDPOINT,
  buildSummary,
  formatBytes,
  parseWorkbook,
} from '../services/excelImport'

const fileInput = ref(null)
const rawFile = shallowRef(null)
const parsed = shallowRef(null)
const activeIndex = ref(0)
const parsing = ref(false)
const errorMsg = ref('')
const dragging = ref(false)
const reviewed = ref(false)
const submit = ref({ status: 'idle', message: '' })

const activeSheet = computed(() => parsed.value?.sheets[activeIndex.value] ?? null)

// Workbook tanpa sheet wajib pasti ditolak converter, jadi jangan biarkan terkirim.
const blockedByMissing = computed(() => (parsed.value?.missingSheets.length ?? 0) > 0)

// Submit hanya terbuka setelah preview berhasil dimuat DAN dikonfirmasi user.
const canSubmit = computed(
  () =>
    !!parsed.value &&
    reviewed.value &&
    !blockedByMissing.value &&
    submit.value.status !== 'sending'
)

function reset() {
  rawFile.value = null
  parsed.value = null
  activeIndex.value = 0
  errorMsg.value = ''
  reviewed.value = false
  submit.value = { status: 'idle', message: '' }
  if (fileInput.value) fileInput.value.value = ''
}

async function handleFiles(files) {
  const file = files?.[0]
  if (!file) return

  reset()
  parsing.value = true
  try {
    rawFile.value = file
    parsed.value = await parseWorkbook(file)
  } catch (err) {
    rawFile.value = null
    errorMsg.value = err.message
  } finally {
    parsing.value = false
  }
}

function onDrop(evt) {
  dragging.value = false
  handleFiles(evt.dataTransfer?.files)
}

async function onSubmit() {
  if (!canSubmit.value) return
  submit.value = { status: 'sending', message: '' }
  try {
    const { submitWorkbook } = await import('../services/excelImport')
    const res = await submitWorkbook(rawFile.value, buildSummary(parsed.value))
    submit.value = {
      status: 'success',
      message: res?.message ?? 'Workbook berhasil dikirim ke server.',
    }
  } catch (err) {
    submit.value = {
      status: err instanceof EndpointNotConfiguredError ? 'not-configured' : 'error',
      message: err.message,
    }
  }
}
</script>

<template>
  <div class="master-page">
    <PageHeader
      title="Import Excel"
      subtitle="Unggah workbook V&V, periksa seluruh sheet pada preview, lalu kirim ke server. Data belum dikirim sebelum Anda menekan tombol submit."
    >
      <template #right>
        <span v-if="parsed" class="stat-chip">Sheet <strong>{{ parsed.sheets.length }}</strong></span>
        <span v-if="parsed" class="stat-chip">Total baris <strong>{{ parsed.totalRows }}</strong></span>
      </template>
    </PageHeader>

    <!-- langkah 1: pilih file -->
    <section
      v-if="!parsed"
      class="panel dz"
      :class="{ 'dz--over': dragging, 'dz--busy': parsing }"
      @dragover.prevent="dragging = true"
      @dragleave.prevent="dragging = false"
      @drop.prevent="onDrop"
    >
      <span class="dz__icon"><DashIcon name="upload" :size="34" /></span>
      <p class="dz__title">
        {{ parsing ? 'Membaca workbook…' : 'Tarik file Excel ke sini' }}
      </p>
      <p class="dz__hint">Format .xlsx, .xlsm, atau .xls — maksimal 25 MB</p>
      <button type="button" class="dz__btn" :disabled="parsing" @click="fileInput.click()">
        Pilih file
      </button>
      <input
        ref="fileInput"
        type="file"
        class="dz__input"
        accept=".xlsx,.xlsm,.xls"
        @change="handleFiles($event.target.files)"
      />
      <p v-if="errorMsg" class="dz__error">
        <DashIcon name="warning" :size="15" /> {{ errorMsg }}
      </p>
    </section>

    <!-- langkah 2: preview -->
    <template v-else>
      <section class="panel filebar">
        <span class="filebar__icon"><DashIcon name="file" :size="20" /></span>
        <div class="filebar__id">
          <strong :title="parsed.fileName">{{ parsed.fileName }}</strong>
          <small>
            {{ formatBytes(parsed.fileSize) }} · {{ parsed.sheets.length }} sheet ·
            {{ parsed.totalRows }} baris
          </small>
        </div>
        <button type="button" class="btn-ghost" @click="reset">Ganti file</button>
      </section>

      <div
        v-if="parsed.missingSheets.length"
        class="notice notice--warn"
        role="alert"
      >
        <DashIcon name="warning" :size="17" />
        <div>
          <strong>{{ parsed.missingSheets.length }} sheet wajib tidak ditemukan.</strong>
          Dashboard tidak dapat dibangun ulang dari file ini sampai sheet berikut tersedia:
          <span class="notice__list">{{ parsed.missingSheets.map((s) => s.name).join(', ') }}</span>
        </div>
      </div>
      <div v-else class="notice notice--ok">
        <DashIcon name="shield" :size="17" />
        <div>
          Seluruh <strong>{{ parsed.sheets.filter((s) => s.required).length }} sheet wajib</strong>
          tersedia. Workbook siap diproses server.
        </div>
      </div>

      <div class="preview">
        <aside class="panel sheetlist">
          <p class="sheetlist__title">Daftar Sheet ({{ parsed.sheets.length }})</p>
          <ul>
            <li v-for="(sheet, i) in parsed.sheets" :key="sheet.name">
              <button
                type="button"
                :class="{ 'is-active': i === activeIndex }"
                @click="activeIndex = i"
              >
                <span class="sheetlist__dot" :class="sheet.required ? 'is-req' : 'is-extra'"></span>
                <span class="sheetlist__name" :title="sheet.name">{{ sheet.name }}</span>
                <span class="sheetlist__count">{{ sheet.rowCount }}</span>
              </button>
            </li>
          </ul>
        </aside>

        <div class="panel preview__pane">
          <SheetPreview v-if="activeSheet" :sheet="activeSheet" />
        </div>
      </div>

      <!-- langkah 3: submit -->
      <section class="panel submitbar">
        <label class="submitbar__check" :class="{ 'is-disabled': blockedByMissing }">
          <input v-model="reviewed" type="checkbox" :disabled="blockedByMissing" />
          <span>Saya sudah memeriksa preview seluruh sheet dan data sudah benar.</span>
        </label>

        <span v-if="blockedByMissing" class="submitbar__blocked">
          Submit dinonaktifkan sampai sheet wajib lengkap.
        </span>

        <button type="button" class="btn-primary" :disabled="!canSubmit" @click="onSubmit">
          {{ submit.status === 'sending' ? 'Mengirim…' : 'Submit ke Server' }}
        </button>
      </section>

      <div v-if="submit.status === 'success'" class="notice notice--ok">
        <DashIcon name="shield" :size="17" />
        <div>{{ submit.message }}</div>
      </div>

      <div v-else-if="submit.status === 'error'" class="notice notice--err" role="alert">
        <DashIcon name="warning" :size="17" />
        <div>{{ submit.message }}</div>
      </div>

      <div v-else-if="submit.status === 'not-configured'" class="notice notice--warn" role="alert">
        <DashIcon name="warning" :size="17" />
        <div>
          <strong>Endpoint backend belum dikonfigurasi — file belum terkirim.</strong>
          Preview di atas berjalan sepenuhnya di browser. Untuk mengaktifkan pengiriman, isi
          <code>VITE_IMPORT_ENDPOINT</code> pada file <code>.env</code>, lalu jalankan ulang dev
          server. File akan dikirim sebagai <code>multipart/form-data</code> dengan field
          <code>file</code> dan <code>summary</code>.
        </div>
      </div>
      <p v-else-if="!IMPORT_ENDPOINT" class="submitbar__note">
        Catatan: endpoint backend belum dikonfigurasi, sehingga submit akan memberi tahu cara
        mengaktifkannya.
      </p>
    </template>
  </div>
</template>

<style scoped>
/* ===== dropzone ===== */
.dz {
  align-items: center;
  padding: 3rem 1.5rem;
  text-align: center;
  border-style: dashed;
  border-width: 2px;
  transition: border-color 0.15s, background 0.15s;
}

.dz--over {
  border-color: var(--accent-blue);
  background: #f4f7ff;
}

.dz--busy {
  opacity: 0.75;
}

.dz__icon {
  color: var(--accent-blue);
  margin-bottom: 0.6rem;
}

.dz__title {
  margin: 0;
  font-size: 1rem;
  font-weight: 800;
  color: var(--ink-strong);
}

.dz__hint {
  margin: 0.25rem 0 1rem;
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--ink-muted);
}

.dz__input {
  display: none;
}

.dz__error {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  margin: 1rem 0 0;
  padding: 0.45rem 0.8rem;
  border-radius: 10px;
  background: #fdeae8;
  color: #b3261e;
  font-size: 0.72rem;
  font-weight: 700;
}

.dz__btn,
.btn-primary {
  border: none;
  border-radius: 10px;
  background: var(--navy-bar);
  color: #fff;
  font-family: inherit;
  font-size: 0.76rem;
  font-weight: 800;
  padding: 0.55rem 1.4rem;
}

.dz__btn:hover:not(:disabled),
.btn-primary:hover:not(:disabled) {
  background: var(--ink-strong);
}

.btn-primary:disabled {
  background: #aab3c9;
}

.btn-ghost {
  margin-left: auto;
  border: 1px solid var(--line);
  border-radius: 10px;
  background: #fff;
  color: var(--ink);
  font-family: inherit;
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.4rem 0.9rem;
}

.btn-ghost:hover {
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

/* ===== bar file terpilih ===== */
.filebar {
  flex-direction: row;
  align-items: center;
  gap: 0.75rem;
  padding: 0.7rem 0.9rem;
  margin-bottom: 0.8rem;
}

.filebar__icon {
  flex: none;
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  border-radius: 10px;
  background: #eef1fb;
  color: var(--navy-bar);
}

.filebar__id {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.filebar__id strong {
  font-size: 0.8rem;
  font-weight: 800;
  color: var(--ink-strong);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.filebar__id small {
  font-size: 0.66rem;
  font-weight: 600;
  color: var(--ink-muted);
}

/* ===== notice ===== */
.notice {
  display: flex;
  align-items: flex-start;
  gap: 0.6rem;
  padding: 0.7rem 0.9rem;
  border-radius: 12px;
  border: 1px solid;
  font-size: 0.72rem;
  font-weight: 600;
  line-height: 1.5;
  margin-bottom: 0.8rem;
}

.notice code {
  background: rgba(20, 36, 107, 0.08);
  border-radius: 5px;
  padding: 0.05rem 0.3rem;
  font-size: 0.95em;
}

.notice--ok {
  background: #e9f6ee;
  border-color: #b6ddc6;
  color: #0a6b38;
}

.notice--warn {
  background: #fdf6e3;
  border-color: #e9d59a;
  color: #7a5800;
}

.notice--err {
  background: #fdeae8;
  border-color: #f0bdb8;
  color: #b3261e;
}

.notice__list {
  display: block;
  margin-top: 0.2rem;
  font-weight: 800;
  word-break: break-word;
}

/* ===== area preview ===== */
.preview {
  display: grid;
  grid-template-columns: 268px minmax(0, 1fr);
  gap: 0.8rem;
  align-items: start;
  margin-bottom: 0.8rem;
}

.sheetlist {
  padding: 0.55rem;
  max-height: 74vh;
  overflow-y: auto;
}

.sheetlist__title {
  margin: 0.15rem 0 0.45rem;
  padding: 0 0.4rem;
  font-size: 0.6rem;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--ink-muted);
}

.sheetlist ul {
  list-style: none;
  margin: 0;
  padding: 0;
}

.sheetlist button {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0.42rem 0.5rem;
  margin-bottom: 2px;
  border: none;
  border-radius: 9px;
  background: none;
  text-align: left;
  font-family: inherit;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--ink);
}

.sheetlist button:hover {
  background: #f1f4fb;
}

.sheetlist button.is-active {
  background: var(--navy-bar);
  color: #fff;
}

.sheetlist__dot {
  flex: none;
  width: 7px;
  height: 7px;
  border-radius: 50%;
}

.sheetlist__dot.is-req {
  background: var(--st-effective);
}

.sheetlist__dot.is-extra {
  background: var(--st-nodata);
}

.sheetlist__name {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sheetlist__count {
  font-size: 0.62rem;
  font-weight: 800;
  opacity: 0.7;
  font-variant-numeric: tabular-nums;
}

.preview__pane {
  min-width: 0;
}

/* ===== submit ===== */
.submitbar {
  flex-direction: row;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
  padding: 0.8rem 0.9rem;
  margin-bottom: 0.8rem;
}

.submitbar__check {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.74rem;
  font-weight: 700;
  color: var(--ink);
  cursor: pointer;
}

.submitbar__check input {
  width: 16px;
  height: 16px;
  accent-color: var(--navy-bar);
}

.submitbar__check.is-disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.submitbar__blocked {
  font-size: 0.7rem;
  font-weight: 700;
  color: #7a5800;
}

.submitbar .btn-primary {
  margin-left: auto;
}

.submitbar__note {
  margin: -0.3rem 0 0.8rem;
  font-size: 0.68rem;
  font-weight: 600;
  color: var(--ink-muted);
}

@media (max-width: 1100px) {
  .preview {
    grid-template-columns: 1fr;
  }

  .sheetlist {
    max-height: 240px;
  }
}
</style>
