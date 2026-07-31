// Parsing workbook di browser + pengiriman ke backend.
// xlsx di-load secara dinamis supaya bundle awal aplikasi tetap ringan
// (library-nya besar dan hanya dibutuhkan di halaman Import).

import { REQUIRED_SHEET_NAMES, findMissingSheets } from '../data/sheet-schema'
import { API_BASE } from './api'
import { authHeader, isLoggedIn } from './auth'

const ACCEPTED_EXT = ['.xlsx', '.xlsm', '.xls']
const MAX_SIZE_MB = 25

export class ImportError extends Error {}
export class EndpointNotConfiguredError extends Error {}

/** Import memerlukan sesi login dengan role Administrator atau Verifier. */
export class NotAuthenticatedError extends Error {
  constructor() {
    super('Anda harus masuk terlebih dahulu untuk mengirim workbook ke server.')
    this.name = 'NotAuthenticatedError'
  }
}

export function formatBytes(bytes) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(2)} MB`
}

export function validateFile(file) {
  const ext = file.name.slice(file.name.lastIndexOf('.')).toLowerCase()
  if (!ACCEPTED_EXT.includes(ext)) {
    throw new ImportError(
      `Format "${ext || 'tanpa ekstensi'}" tidak didukung. Gunakan file ${ACCEPTED_EXT.join(', ')}.`
    )
  }
  if (file.size > MAX_SIZE_MB * 1024 * 1024) {
    throw new ImportError(
      `Ukuran file ${formatBytes(file.size)} melebihi batas ${MAX_SIZE_MB} MB.`
    )
  }
  if (file.size === 0) {
    throw new ImportError('File kosong (0 byte).')
  }
}

// Signature file spreadsheet asli. Diperiksa karena SheetJS sangat permisif:
// file teks biasa berekstensi .xlsx akan "berhasil" diparse sebagai CSV dan
// menghasilkan preview palsu yang terlihat sah.
const ZIP_SIGNATURE = [0x50, 0x4b, 0x03, 0x04] // .xlsx / .xlsm (arsip ZIP)
const OLE_SIGNATURE = [0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1] // .xls (BIFF/OLE2)

function hasSignature(bytes, signature) {
  return signature.every((b, i) => bytes[i] === b)
}

function assertSpreadsheetBinary(buffer) {
  const head = new Uint8Array(buffer.slice(0, 8))
  if (!hasSignature(head, ZIP_SIGNATURE) && !hasSignature(head, OLE_SIGNATURE)) {
    throw new ImportError(
      'Isi file bukan workbook Excel yang valid, meskipun ekstensinya benar. ' +
        'File mungkin rusak atau sebenarnya berformat lain (CSV/teks) yang diganti namanya.'
    )
  }
}

/**
 * Membaca SELURUH sheet dalam workbook menjadi bentuk siap-preview.
 * raw:false dipakai supaya sel tampil sama persis seperti di Excel
 * (tanggal tidak muncul sebagai angka serial).
 */
export async function parseWorkbook(file) {
  validateFile(file)

  const buffer = await file.arrayBuffer()
  assertSpreadsheetBinary(buffer)

  const XLSX = await import('xlsx')
  let wb
  try {
    wb = XLSX.read(buffer, { type: 'array' })
  } catch (err) {
    throw new ImportError(`File tidak dapat dibaca sebagai workbook Excel. (${err.message})`)
  }

  if (!wb.SheetNames?.length) {
    throw new ImportError('Workbook tidak memiliki sheet sama sekali.')
  }

  const required = new Set(REQUIRED_SHEET_NAMES)

  const sheets = wb.SheetNames.map((name) => {
    const ws = wb.Sheets[name]
    // blankrows:true dipertahankan supaya indeks larik = nomor baris asli Excel.
    // Baris kosong dibuang belakangan, setelah nomor barisnya dicatat — kalau
    // dibuang oleh SheetJS, nomor baris preview akan bergeser dari file aslinya.
    const raw = XLSX.utils.sheet_to_json(ws, {
      header: 1,
      raw: false,
      defval: '',
      blankrows: true,
    })
    const colCount = raw.reduce((max, r) => Math.max(max, r.length), 0)

    const rows = []
    raw.forEach((cells, i) => {
      // samakan panjang tiap baris agar grid preview tidak bergerigi
      const padded = Array.from({ length: colCount }, (_, c) => cells[c] ?? '')
      if (padded.some((v) => String(v).trim() !== '')) {
        rows.push({ excelRow: i + 1, cells: padded })
      }
    })

    return {
      name,
      rows,
      rowCount: rows.length,
      colCount,
      required: required.has(name),
      empty: rows.length === 0,
    }
  })

  return {
    fileName: file.name,
    fileSize: file.size,
    sheets,
    totalRows: sheets.reduce((sum, s) => sum + s.rowCount, 0),
    missingSheets: findMissingSheets(wb.SheetNames),
  }
}

/**
 * Mengubah state edit UI menjadi daftar datar siap kirim.
 * excelRow/col memakai penomoran Excel (baris 1-based, kolom 1-based) supaya
 * backend bisa menerapkannya langsung ke workbook tanpa menebak offset.
 */
export function flattenEdits(edits) {
  const out = []
  for (const [sheetName, cells] of Object.entries(edits ?? {})) {
    for (const edit of Object.values(cells)) {
      out.push({
        sheet: sheetName,
        excelRow: edit.excelRow,
        excelCol: edit.col + 1,
        cell: `${columnLetter(edit.col)}${edit.excelRow}`,
        from: edit.from,
        to: edit.to,
      })
    }
  }
  return out
}

export function columnLetter(index) {
  let s = ''
  let n = index
  do {
    s = String.fromCharCode(65 + (n % 26)) + s
    n = Math.floor(n / 26) - 1
  } while (n >= 0)
  return s
}

/** Ringkasan yang ikut dikirim ke backend bersama file aslinya. */
export function buildSummary(parsed, edits) {
  const editList = flattenEdits(edits)
  return {
    fileName: parsed.fileName,
    fileSize: parsed.fileSize,
    sheetCount: parsed.sheets.length,
    totalRows: parsed.totalRows,
    sheets: parsed.sheets.map((s) => ({
      name: s.name,
      rows: s.rowCount,
      cols: s.colCount,
      required: s.required,
    })),
    editCount: editList.length,
  }
}

// Endpoint import bawaan mengikuti backend Sifp_Vue.Server. Masih bisa ditimpa
// lewat VITE_IMPORT_ENDPOINT bila workbook dikirim ke layanan lain.
export const IMPORT_ENDPOINT =
  import.meta.env.VITE_IMPORT_ENDPOINT || `${API_BASE}/api/import/excel`

/**
 * Mengirim file asli (bukan hasil parse) ke backend sebagai multipart/form-data,
 * supaya backend tetap menjadi sumber kebenaran saat memproses ulang workbook.
 *
 * Perubahan sel dari preview dikirim TERPISAH sebagai daftar `edits`, bukan
 * ditimpakan ke file. Menyusun ulang .xlsx di browser akan membuang rumus,
 * format, dan merge cell dari workbook asli — dan diam-diam mengubah arti file
 * yang diunggah user. Backend yang menerapkan edits ke file asli.
 */
export async function submitWorkbook(file, summary, edits) {
  if (!IMPORT_ENDPOINT) throw new EndpointNotConfiguredError()
  if (!isLoggedIn.value) throw new NotAuthenticatedError()

  const body = new FormData()
  body.append('file', file, file.name)
  body.append('summary', JSON.stringify(summary))
  body.append('edits', JSON.stringify(flattenEdits(edits)))

  let res
  try {
    // Content-Type tidak diisi manual: browser perlu menuliskannya sendiri
    // lengkap dengan boundary multipart.
    res = await fetch(IMPORT_ENDPOINT, { method: 'POST', body, headers: authHeader() })
  } catch (err) {
    throw new ImportError(`Tidak dapat menghubungi server: ${err.message}`)
  }

  const text = await res.text().catch(() => '')
  let payload = null
  try {
    payload = text ? JSON.parse(text) : null
  } catch {
    payload = null
  }

  if (res.status === 401 || res.status === 403) {
    throw new NotAuthenticatedError()
  }

  // Backend membalas amplop { status, message, data } — pesan darinya jauh lebih
  // berguna daripada sekadar kode HTTP (mis. daftar sheet wajib yang hilang).
  if (!res.ok || payload?.status === 'ERROR') {
    const detail = payload?.message ?? text.slice(0, 200)
    throw new ImportError(detail || `Server menolak import (HTTP ${res.status}).`)
  }

  return payload?.data ?? payload ?? {}
}
