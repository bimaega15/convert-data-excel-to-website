// Daftar sheet yang dibaca oleh scripts/convert-excel.mjs.
// Dipakai bersama oleh converter (preflight check) dan halaman Import Excel
// (validasi sebelum submit) supaya keduanya tidak pernah berbeda.

export const REQUIRED_SHEETS = [
  { name: 'INPUT-SIF_Questions', label: 'Jawaban pertanyaan verifikasi SIF' },
  { name: 'INPUT-Error_Traps', label: 'Error traps per observasi' },
  { name: 'INPUT-HP_Tools', label: 'Human Performance Tools' },
  { name: 'INPUT-Drift_Conditions', label: 'Kondisi drift' },
  { name: 'INPUT-Latent_Conditions', label: 'Kondisi laten' },
  { name: 'DATABASE_PSEC_CCVC', label: 'Master library PSEC & CCVC' },
  { name: 'ANALYZE-CONFORMANCE_SCORE', label: 'Rekap observasi & skor' },
  { name: 'ANALYZE-EXECUTIVE_MEASURES', label: 'KPI PSEC / CCVC / PSIE / Conformance' },
  { name: 'ANALYZE-QUICK_FACTS', label: 'Quick facts dashboard' },
  { name: 'ANALYZE-CLSR_HEALTH_MAP', label: 'Health map CLSR × Zona' },
  { name: 'ANALYZE-TOP5', label: 'Top 5 (exposure, gap, drift, systemic)' },
  { name: 'ANALYZE-TREND_ZONE', label: 'Tren bulanan & skor per zona' },
  { name: 'ANALYZE-IMPROVEMENT_INITIATIVES', label: 'Inisiatif perbaikan' },
  { name: 'CONFIG-DASHBOARD_TEXT', label: 'Teks naratif dashboard' },
]

export const REQUIRED_SHEET_NAMES = REQUIRED_SHEETS.map((s) => s.name)

export function findMissingSheets(sheetNames) {
  const present = new Set(sheetNames)
  return REQUIRED_SHEETS.filter((s) => !present.has(s.name))
}
