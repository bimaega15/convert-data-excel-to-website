// Manifest seluruh worksheet Excel (dihasilkan oleh scripts/convert-excel.mjs).
// Menjadi sumber tunggal daftar menu sidebar dan metadata viewer generik,
// sehingga jumlah menu otomatis mengikuti jumlah worksheet di file Excel.
import manifest from './generated/sheets/_manifest.json'

export const sheetManifest = manifest
export const sheetGroups = manifest.groups

// Peta cepat slug -> entri sheet, dipakai halaman viewer & guard router.
export const sheetBySlug = Object.fromEntries(
  manifest.groups.flatMap((g) => g.items.map((it) => [it.slug, it]))
)
