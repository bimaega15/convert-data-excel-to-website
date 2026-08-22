// Adapter data dashboard: seluruh nilai berasal dari `GET /api/dashboard`
// (dulu dari src/data/generated/dashboard.json hasil `npm run convert:excel`).
//
// Bentuk tiap ekspor sengaja dipertahankan persis seperti versi JSON — objek dan
// array biasa, bukan ref — sehingga komponen dashboard tidak perlu diubah sama
// sekali. Konsekuensinya `loadDashboard()` wajib selesai SEBELUM app di-mount;
// itu dikerjakan di src/main.js.

import { reactive, ref } from 'vue'
import { api } from '../services/api'

export const meta = reactive({
  title: 'REGIONAL 4 SIFP ASSURANCE DASHBOARD',
  subtitle: '',
  draft: false,
  sourceFile: '',
  generatedAt: '',
})

export const kpis = reactive([])
export const conformance = reactive({ value: 0, target: '', bands: [] })
export const quickFacts = reactive([])
export const healthMap = reactive({ zones: [], rows: [] })
export const topPanels = reactive([])
export const trend = reactive({ target: 0, targetLabel: '', points: [], projection: [] })
export const zonaScores = reactive({ target: 0, targetLabel: '', bars: [] })
export const initiatives = reactive([])
export const summaryCards = reactive([])
export const summaryNotes = reactive([])
export const footerNote = ref('')
export const observationsByMonth = reactive([])

/** "Zona 11" / "Z11" -> "Z11" — label ringkas dipakai di kartu/tabel yang sempit
 *  (Zone Scores, Observation by Zone/Month, Quick Facts). ZonaBarDto.Zone bisa
 *  berupa salah satu bentuk tergantung data hasil import Excel. */
export function zoneShort(label) {
  const m = /(\d+)/.exec(label ?? '')
  return m ? `Z${m[1]}` : (label ?? '')
}

export const statusLegend = [
  { status: 'effective', label: 'Effective (≥80%)' },
  { status: 'degraded', label: 'Degraded (50% – <80%)' },
  { status: 'failed', label: 'Failed / High Concern (<50%)' },
  { status: 'nodata', label: 'No Data' },
]

/** Isi array reaktif tanpa mengganti referensinya, agar snapshot di komponen tetap sah. */
function fill(target, rows) {
  target.splice(0, target.length, ...(rows ?? []))
}

export async function loadDashboard() {
  const data = await api.get('/api/dashboard')

  Object.assign(meta, data.meta ?? {})
  fill(kpis, data.kpis)
  Object.assign(conformance, data.conformance ?? {})
  fill(quickFacts, data.quickFacts)

  Object.assign(healthMap, { zones: data.healthMap?.zones ?? [], rows: data.healthMap?.rows ?? [] })
  fill(topPanels, data.topPanels)

  Object.assign(trend, {
    target: data.trend?.target ?? 0,
    targetLabel: data.trend?.targetLabel ?? '',
    points: data.trend?.points ?? [],
    projection: data.trend?.projection ?? [],
  })

  Object.assign(zonaScores, {
    target: data.zonaScores?.target ?? 0,
    targetLabel: data.zonaScores?.targetLabel ?? '',
    bars: data.zonaScores?.bars ?? [],
  })

  fill(initiatives, data.initiatives)
  fill(summaryCards, data.summaryCards)
  fill(summaryNotes, data.summaryNotes)
  footerNote.value = data.footerNote ?? ''
  fill(observationsByMonth, data.observationsByMonth)

  return data
}
