// Parsing workbook di browser + pengiriman ke backend.
// xlsx di-load secara dinamis supaya bundle awal aplikasi tetap ringan
// (library-nya besar dan hanya dibutuhkan di halaman Import).

import { REQUIRED_SHEET_NAMES, findMissingSheets } from '../data/sheet-schema'

const ACCEPTED_EXT = ['.xlsx', '.xlsm', '.xls']
const MAX_SIZE_MB = 25

export class ImportError extends Error {}
export class EndpointNotConfiguredError extends Error {}

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
    const rows = XLSX.utils.sheet_to_json(ws, {
      header: 1,
      raw: false,
      defval: '',
      blankrows: false,
    })
    const colCount = rows.reduce((max, r) => Math.max(max, r.length), 0)
    // samakan panjang tiap baris agar grid preview tidak bergerigi
    const grid = rows.map((r) => Array.from({ length: colCount }, (_, i) => r[i] ?? ''))

    return {
      name,
      grid,
      rowCount: grid.length,
      colCount,
      required: required.has(name),
      empty: grid.length === 0,
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

/** Ringkasan yang ikut dikirim ke backend bersama file aslinya. */
export function buildSummary(parsed) {
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
  }
}

export const IMPORT_ENDPOINT = import.meta.env.VITE_IMPORT_ENDPOINT ?? ''

/**
 * Mengirim file asli (bukan hasil parse) ke backend sebagai multipart/form-data,
 * supaya backend tetap menjadi sumber kebenaran saat memproses ulang workbook.
 */
export async function submitWorkbook(file, summary) {
  if (!IMPORT_ENDPOINT) throw new EndpointNotConfiguredError()

  const body = new FormData()
  body.append('file', file, file.name)
  body.append('summary', JSON.stringify(summary))

  let res
  try {
    res = await fetch(IMPORT_ENDPOINT, { method: 'POST', body })
  } catch (err) {
    throw new ImportError(`Tidak dapat menghubungi server: ${err.message}`)
  }

  if (!res.ok) {
    const detail = await res.text().catch(() => '')
    throw new ImportError(
      `Server menolak import (HTTP ${res.status}).${detail ? ` ${detail.slice(0, 200)}` : ''}`
    )
  }

  return res.json().catch(() => ({}))
}
