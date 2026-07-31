// Manifest seluruh worksheet Excel, kini berasal dari `GET /api/worksheets/manifest`
// (dulu dari src/data/generated/sheets/_manifest.json).
//
// Menjadi sumber tunggal daftar menu sidebar dan metadata viewer generik, sehingga
// jumlah menu otomatis mengikuti worksheet pada workbook yang terakhir diimport.
// Sama seperti data dashboard, manifest dimuat sebelum app di-mount (src/main.js)
// karena sidebar dan router membacanya secara sinkron.

import { reactive } from 'vue'
import { api } from '../services/api'

export const sheetManifest = reactive({
  generatedAt: '',
  sourceFile: '',
  sheetCount: 0,
  groups: [],
})

export const sheetGroups = reactive([])

/** Peta cepat slug -> entri sheet, dipakai halaman viewer & guard router. */
export const sheetBySlug = reactive({})

export async function loadSheetManifest() {
  const data = await api.get('/api/worksheets/manifest')

  Object.assign(sheetManifest, {
    generatedAt: data.generatedAt ?? '',
    sourceFile: data.sourceFile ?? '',
    sheetCount: data.sheetCount ?? 0,
    groups: data.groups ?? [],
  })

  sheetGroups.splice(0, sheetGroups.length, ...(data.groups ?? []))

  for (const key of Object.keys(sheetBySlug)) delete sheetBySlug[key]
  for (const group of data.groups ?? []) {
    for (const item of group.items ?? []) sheetBySlug[item.slug] = item
  }

  return data
}
