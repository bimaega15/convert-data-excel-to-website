// Daftar sheet WAJIB pada template resmi (SifpAssurance_Template.xlsm) yang
// diupload pengguna. Dipakai halaman Import Excel untuk validasi sebelum submit,
// dan HARUS identik dengan Sifp_Vue.Server/Helpers/SheetSchema.cs (gerbang server).

export const REQUIRED_SHEETS = [
  { name: 'SIF Questions', label: 'Jawaban pertanyaan verifikasi SIF' },
  { name: 'Error Traps', label: 'Error traps per observasi' },
  { name: 'HP Tools', label: 'Human Performance Tools' },
  { name: 'Drift Conditions', label: 'Kondisi drift' },
  { name: 'Latent Conditions', label: 'Kondisi laten' },
  { name: 'PSEC CCVC', label: 'Master library PSEC & CCVC' },
  { name: 'Conformance Score', label: 'Rekap observasi & skor' },
  { name: 'Executive Measures', label: 'KPI PSEC / CCVC / PSIE / Conformance' },
  { name: 'Quick Facts', label: 'Quick facts dashboard' },
  { name: 'CLSR Health', label: 'Health map CLSR × Zona' },
  { name: 'Top 5', label: 'Top 5 (exposure, gap, drift, systemic)' },
  { name: 'Trend Zone', label: 'Tren bulanan & skor per zona' },
  { name: 'Improvement Initiatives', label: 'Inisiatif perbaikan' },
  { name: 'Dashboard Text', label: 'Teks naratif dashboard' },
]

export const REQUIRED_SHEET_NAMES = REQUIRED_SHEETS.map((s) => s.name)

export function findMissingSheets(sheetNames) {
  const present = new Set(sheetNames)
  return REQUIRED_SHEETS.filter((s) => !present.has(s.name))
}
