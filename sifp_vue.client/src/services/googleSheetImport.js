// Mengunduh Google Sheets publik sebagai XLSX, lalu mem-parse-nya
// menggunakan SheetJS — menghasilkan struktur data yang SAMA dengan
// parseWorkbook() sehingga komponen SheetPreview bisa dipakai tanpa modifikasi.

import { REQUIRED_SHEET_NAMES, findMissingSheets } from '../data/sheet-schema'

/**
 * Mengekstrak Spreadsheet ID dari berbagai format URL Google Sheets.
 * Mendukung:
 *   - https://docs.google.com/spreadsheets/d/{ID}/edit
 *   - https://docs.google.com/spreadsheets/d/{ID}/pub
 *   - https://docs.google.com/spreadsheets/d/{ID}
 *   - https://docs.google.com/spreadsheets/d/{ID}/edit?gid=...
 *   - Link share biasa apa pun asal mengandung /spreadsheets/d/{ID}
 */
export function parseSpreadsheetId(url) {
  const trimmed = (url ?? '').trim()
  const match = trimmed.match(/\/spreadsheets\/d\/([a-zA-Z0-9_-]+)/)
  if (!match) {
    throw new Error(
      'URL bukan link Google Sheets yang valid. ' +
        'Pastikan URL mengandung /spreadsheets/d/ dan coba lagi.'
    )
  }
  return match[1]
}

/**
 * Entry point utama. Mengunduh spreadsheet publik Google sebagai XLSX,
 * mem-parse-nya, dan mengembalikan objek dengan format identik dengan
 * output parseWorkbook() (ditambah flag isGoogleSheet = true).
 *
 * @param {string} url  URL Google Sheets (format apa pun)
 * @returns {Promise<object>}  Parsed workbook siap dipakai ImportPage
 */
export async function loadGoogleSheet(url) {
  const id = parseSpreadsheetId(url)

  // Google menyediakan endpoint export XLSX untuk spreadsheet publik.
  // Fetch mengikuti redirect otomatis; CORS diizinkan oleh Google untuk
  // spreadsheet yang bersifat publik (Anyone with the link can view).
  const exportUrl = `https://docs.google.com/spreadsheets/d/${id}/export?format=xlsx`

  let buffer
  try {
    const res = await fetch(exportUrl, { mode: 'cors' })
    if (!res.ok) {
      if (res.status === 403 || res.status === 401) {
        throw new Error(
          'Spreadsheet tidak dapat diakses. ' +
            'Pastikan spreadsheet bersifat publik: Bagikan → "Anyone with the link" → Viewer.'
        )
      }
      throw new Error(
        `Google menolak permintaan (HTTP ${res.status}). ` +
          'Coba periksa kembali link dan akses spreadsheet.'
      )
    }
    buffer = await res.arrayBuffer()
  } catch (err) {
    // TypeError biasanya muncul saat network error atau CORS block
    if (err instanceof TypeError || err.message.toLowerCase().includes('failed to fetch')) {
      throw new Error(
        'Tidak dapat mengunduh spreadsheet. ' +
          'Pastikan spreadsheet bersifat publik dan koneksi internet tersedia.'
      )
    }
    throw err
  }

  // Parse buffer dengan SheetJS (sama persis seperti parseWorkbook di excelImport.js)
  const XLSX = await import('xlsx')
  let wb
  try {
    wb = XLSX.read(buffer, { type: 'array' })
  } catch (err) {
    throw new Error(
      `File dari Google Sheets tidak dapat dibaca sebagai workbook. (${err.message})`
    )
  }

  if (!wb.SheetNames?.length) {
    throw new Error('Spreadsheet tidak memiliki sheet.')
  }

  const required = new Set(REQUIRED_SHEET_NAMES)

  const sheets = wb.SheetNames.map((name) => {
    const ws = wb.Sheets[name]
    // blankrows:true dipertahankan agar indeks array = nomor baris Excel asli.
    const raw = XLSX.utils.sheet_to_json(ws, {
      header: 1,
      raw: false,
      defval: '',
      blankrows: true,
    })
    const colCount = raw.reduce((max, r) => Math.max(max, r.length), 0)

    const rows = []
    raw.forEach((cells, i) => {
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
    fileName: `Google Sheets`,
    fileSize: buffer.byteLength,
    sheets,
    totalRows: sheets.reduce((sum, s) => sum + s.rowCount, 0),
    missingSheets: findMissingSheets(wb.SheetNames),
    // Flag khusus untuk membedakan mode ini di ImportPage
    isGoogleSheet: true,
    spreadsheetId: id,
    spreadsheetUrl: url.trim(),
  }
}
