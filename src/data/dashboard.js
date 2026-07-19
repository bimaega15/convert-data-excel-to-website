// Adapter data dashboard: seluruh nilai berasal dari hasil konversi Excel
// (src/data/generated/dashboard.json). Jalankan `npm run convert:excel`
// setiap kali file Excel di folder design/ diperbarui.
import generated from './generated/dashboard.json'

export const meta = generated.meta
export const kpis = generated.kpis
export const conformance = generated.conformance
export const quickFacts = generated.quickFacts
export const healthMap = generated.healthMap
export const topPanels = generated.topPanels
export const trend = generated.trend
export const zonaScores = generated.zonaScores
export const initiatives = generated.initiatives
export const summaryCards = generated.summaryCards
export const summaryNotes = generated.summaryNotes
export const footerNote = generated.footerNote

export const statusLegend = [
  { status: 'effective', label: 'Effective (≥80%)' },
  { status: 'degraded', label: 'Degraded (50% – <80%)' },
  { status: 'failed', label: 'Failed / High Concern (<50%)' },
  { status: 'nodata', label: 'No Data' },
]
