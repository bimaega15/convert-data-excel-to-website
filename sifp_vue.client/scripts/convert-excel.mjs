// Konversi workbook V&V (Excel) menjadi JSON di src/data/generated/.
// Jalankan ulang setiap file Excel diperbarui:
//   npm run convert:excel            -> pakai file default di design/
//   npm run convert:excel -- <path>  -> pakai file lain
import { createRequire } from 'node:module'
import { mkdirSync, writeFileSync, rmSync } from 'node:fs'
import { basename, join, dirname } from 'node:path'
import { fileURLToPath } from 'node:url'
import { findMissingSheets, REQUIRED_SHEET_NAMES } from '../src/data/sheet-schema.js'

const require = createRequire(import.meta.url)
const XLSX = require('xlsx')

const root = join(dirname(fileURLToPath(import.meta.url)), '..')
const srcFile =
  process.argv[2] ?? join(root, 'design', 'VnV_FULL_DATABASE_09July2026_OBS001-023 ver.Jul2026.xlsx')
const outDir = join(root, 'src', 'data', 'generated')

// cellDates dimatikan: serial Excel dikonversi manual berbasis UTC supaya
// tanggal tidak bergeser oleh penyesuaian zona waktu lokal SheetJS.
const wb = XLSX.readFile(srcFile)

// Gagal lebih awal dengan pesan jelas, bukan crash saat membaca sheet yang hilang.
const missing = findMissingSheets(wb.SheetNames)
if (missing.length) {
  console.error(`\n✗ Workbook tidak lengkap: ${missing.length} sheet wajib tidak ditemukan.\n`)
  for (const s of missing) console.error(`  - ${s.name}  (${s.label})`)
  console.error(`\nSumber: ${basename(srcFile)}\n`)
  process.exit(1)
}

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
const EXCEL_EPOCH_UTC = Date.UTC(1899, 11, 30)

function serialToDate(serial, roundFn) {
  if (typeof serial !== 'number') return null
  return new Date(EXCEL_EPOCH_UTC + roundFn(serial) * 86400000)
}

// Serial di workbook ini berupa bilangan bulat (tengah malam); round menjaga
// dari file lain yang menyimpan tanggal dengan komponen waktu.
function isoDate(serial) {
  const d = serialToDate(serial, Math.round)
  return d ? d.toISOString().slice(0, 10) : null
}

// Baris trend adalah penanda awal bulan (1 Mei = data bulan Mei).
function monthLabel(serial) {
  const d = serialToDate(serial, Math.floor)
  return d ? `${MONTHS[d.getUTCMonth()]}-${String(d.getUTCFullYear()).slice(2)}` : null
}

function pct(x, digits = 2) {
  if (x == null || x === '') return null
  const n = Number(x)
  if (Number.isNaN(n)) return null
  return Math.round(n * 100 * 10 ** digits) / 10 ** digits
}

function objRows(sheet) {
  return XLSX.utils.sheet_to_json(wb.Sheets[sheet], { defval: null })
}

function gridRows(sheet) {
  return XLSX.utils.sheet_to_json(wb.Sheets[sheet], { header: 1, defval: null, blankrows: false })
}

const clean = (s) => (typeof s === 'string' ? s.trim() : s)

// ---------- Master data ----------

const observations = objRows('ANALYZE-CONFORMANCE_SCORE')
  .filter((r) => r.Obs_ID)
  .map((r) => ({
    id: r.Obs_ID,
    protocolCode: r.Protocol_Code,
    protocolName: r.Protocol_Name,
    date: isoDate(r.Observation_Date),
    zona: r.Zona,
    site: r.Site,
    area: r.Area_Equipment,
    activity: r.Activity,
    company: r.Company,
    observers: [r.Observer_1, r.Observer_2, r.Observer_3].filter(Boolean),
    yes: r.YES_Count,
    no: r.NO_Count,
    na: r.NA_Count,
    performance: pct(r['Performance_%']),
    sequence: r.Observation_Sequence,
    psieEligible: r.PSIE_Eligible,
    status: r.Observation_Status,
    active: r.Active_Observation,
  }))

const sifQuestions = objRows('INPUT-SIF_Questions')
  .filter((r) => r.Obs_ID)
  .map((r) => ({
    obsId: r.Obs_ID,
    protocolCode: r.Protocols_code,
    protocolName: r.Protocols_name,
    questionRef: r.Question_Ref,
    ccvcId: r.CCVC_ID,
    question: clean(r.Observation_Question),
    answer: r.YES ? 'YES' : r.NO ? 'NO' : r.NA ? 'NA' : '-',
    comments: clean(r.Comments),
    sifExposure: r.SIF_Exposure,
    criticalSafeguard: r.Critical_Safeguard,
    date: isoDate(r.Observation_Date),
    zona: r.Zona,
    site: r.Site,
    activity: r.Activity,
    company: r.Company,
  }))

const ccvcLibrary = objRows('DATABASE_PSEC_CCVC')
  .filter((r) => r.CCVC_ID)
  .map((r) => ({
    no: r.No,
    protocolGroup: r.Protocol_Group,
    psecId: r.PSEC_ID,
    psecName: r.PSEC_Name,
    exposureType: r.Exposure_Type,
    ccvcId: r.CCVC_ID,
    questionCode: r.Question_Code,
    questionSummary: clean(r.Question_Summary),
    verificationPurpose: clean(r.Verification_Purpose),
  }))

// Sheet berikut punya header ganda ("Protocols" dua kali) -> mapping per indeks.
const errorTraps = gridRows('INPUT-Error_Traps')
  .slice(1)
  .filter((r) => r[0])
  .map((r) => ({
    obsId: r[0],
    protocolCode: r[1],
    protocolName: r[2],
    category: r[3],
    errorTrap: r[4],
    comments: clean(r[5]),
  }))

const hpTools = gridRows('INPUT-HP_Tools')
  .slice(1)
  .filter((r) => r[0])
  .map((r) => ({
    obsId: r[0],
    protocolCode: r[1],
    protocolName: r[2],
    tool: r[3],
    tujuan: clean(r[4]),
    kapanDigunakan: clean(r[5]),
    caraPakai: clean(r[6]),
    effectivenessNotes: clean(r[7]),
  }))

const driftConditions = gridRows('INPUT-Drift_Conditions')
  .slice(1)
  .filter((r) => r[0])
  .map((r) => ({
    obsId: r[0],
    protocolCode: r[1],
    protocolName: r[2],
    situation: clean(r[3]),
    level1: r[4],
    code: r[5],
    level2: r[6],
    reason: clean(r[7]),
    sequence: r[8],
    status: r[9],
    active: r[10],
  }))

const latentConditions = gridRows('INPUT-Latent_Conditions')
  .slice(1)
  .filter((r) => r[0])
  .map((r) => ({
    obsId: r[0],
    protocolCode: r[1],
    protocolName: r[2],
    observation: clean(r[3]),
    level1: r[4],
    code: r[5],
    level2: r[6],
    reason: clean(r[7]),
    sequence: r[10],
    status: r[11],
    active: r[12],
  }))

const initiatives = objRows('ANALYZE-IMPROVEMENT_INITIATIVES')
  .filter((r) => r.Improvement_ID)
  .map((r) => ({
    id: r.Improvement_ID,
    initiative: clean(r.Initiative),
    relatedClsr: r.Related_CLSR,
    owner: r['V&V_Team_Asset_Owner'],
    status: r.Status,
    progress: pct(r['Progress_%'], 0),
    expectedImpact: clean(r.Expected_Impact),
    notes: clean(r.Notes),
  }))

// ---------- Data dashboard (dari sheet ANALYZE) ----------

const measures = {}
for (const r of objRows('ANALYZE-EXECUTIVE_MEASURES').filter((r) => r.Metric_Code)) {
  measures[r.Metric_Code] = {
    name: r.Metric_Name,
    numerator: r.Numerator,
    denominator: r.Denominator,
    value: pct(r['Score_%']),
    target: pct(r['Target_%'], 0),
    status: r.Status,
    notes: r.Notes,
  }
}

const quickFactIcons = {
  'Total Observations Completed': 'clipboard',
  'Priority SIF Exposure Verified (PSEC)': 'shield',
  'Critical Safeguards Verified (CCVC)': 'checklist',
  'Regional 4 Conformance Score': 'gear',
  'Zones Covered': 'pin',
  'Observation Period': 'calendar',
  'Sites / Locations Observed': 'pin',
  'Missing Zones': 'warning',
}

const quickFacts = objRows('ANALYZE-QUICK_FACTS')
  .filter((r) => r.Fact_Name)
  .map((r) => ({
    icon: quickFactIcons[r.Fact_Name] ?? 'clipboard',
    label: r.Fact_Name,
    value:
      typeof r.Fact_Value === 'number' && r.Fact_Value <= 1
        ? `${pct(r.Fact_Value)}%`
        : String(r.Fact_Value),
  }))

const STATUS_KEY = {
  Effective: 'effective',
  Degraded: 'degraded',
  'Failed / High Concern': 'failed',
  'No Data': 'nodata',
}

const healthRows = objRows('ANALYZE-CLSR_HEALTH_MAP')
  .filter((r) => r.CLSR_ID)
  .map((r) => {
    const cells = [11, 12, 13, 14].map((z) => {
      const status = STATUS_KEY[r[`Zona_${z}_Status`]] ?? 'nodata'
      const score = pct(r[`Zona_${z}_Score`])
      return {
        status,
        score,
        // angka ditampilkan di sel hanya untuk status failed (mengikuti desain)
        value: status === 'failed' ? score : null,
      }
    })
    return {
      name: r.CLSR_Description,
      cells,
      regional: pct(r.Regional_4_Score),
      regionalStatus: STATUS_KEY[r.Health_Status] ?? 'nodata',
    }
  })

const top5Raw = objRows('ANALYZE-TOP5').filter((r) => r.Category)
const byCategory = (cat) => top5Raw.filter((r) => r.Category === cat)

function topPanel(no, cat, { title, subtitle, variant, dash, footerIcon, footerLabel, withPercent }) {
  const rows = byCategory(cat)
  const maxCount = Math.max(...rows.map((r) => r.Count), 1)
  return {
    no,
    title,
    subtitle,
    variant,
    dash,
    items: rows.map((r) => ({
      label: r.Item,
      display: withPercent ? `${r.Count} (${Math.round(r.Percent * 100)}%)` : String(r.Count),
      weight: r.Count / maxCount,
    })),
    footer: { icon: footerIcon, label: footerLabel, value: rows[0]?.Denominator ?? 0 },
  }
}

const topPanels = [
  topPanel(2, 'Top SIF Exposure', {
    title: 'TOP 5 SIF EXPOSURES',
    subtitle: '(by Frequency)',
    variant: 'green',
    dash: 'green',
    footerIcon: 'clipboard',
    footerLabel: 'Total Observations',
    withPercent: true,
  }),
  topPanel(3, 'Top Critical Safeguard Gap', {
    title: 'TOP 5 CRITICAL SAFEGUARD GAPS',
    subtitle: '(by Frequency)',
    variant: 'red',
    dash: 'red',
    footerIcon: 'shield',
    footerLabel: 'Total Findings',
    withPercent: false,
  }),
  topPanel(4, 'Top Recurring Drift', {
    title: 'TOP 5 RECURRING DRIFT',
    subtitle: '(Observed)',
    variant: 'blue',
    dash: 'amber',
    footerIcon: 'refresh',
    footerLabel: 'Total Occurrences',
    withPercent: false,
  }),
  topPanel(5, 'Top Systemic Issue', {
    title: 'TOP 5 SYSTEMIC ISSUES',
    subtitle: '(Identified)',
    variant: 'purple',
    dash: null,
    footerIcon: 'gear',
    footerLabel: 'Total Findings',
    withPercent: false,
  }),
]

const trendGrid = gridRows('ANALYZE-TREND_ZONE').slice(1)
const trendActual = []
const trendProjection = []
for (const r of trendGrid) {
  const label = monthLabel(r[0])
  if (!label) continue
  const actual = pct(r[4])
  const planned = pct(r[5] === 'N/A' ? null : r[5])
  if (actual != null && r[6] > 0) {
    trendActual.push({ month: label, value: actual })
  } else if (actual == null && planned != null && trendActual.length > 0) {
    trendProjection.push({ month: label, value: planned })
  }
}

const zonaScores = trendGrid
  .filter((r) => r[8] != null)
  .map((r) => ({
    zone: `Zona ${r[8]}`,
    obs: r[13] ?? 0,
    value: pct(r[12]) ?? 0,
  }))

const configText = {}
for (const r of objRows('CONFIG-DASHBOARD_TEXT').filter((r) => r.Section)) {
  configText[r.Section] = r.Text
}

const topSystemic = byCategory('Top Systemic Issue').slice(0, 3).map((r) => r.Item)
const summaryCards = [
  { icon: 'warning', tone: 'red', title: 'TOP SIF EXPOSURES', text: configText['Top SIF Exposure Note'] },
  { icon: 'shield', tone: 'navy', title: 'CRITICAL GAPS', text: configText['Critical Gaps Note'] },
  { icon: 'pin', tone: 'red', title: 'ZONA ATTENTION', text: configText['Zona Attention Note'] },
  {
    icon: 'people',
    tone: 'navy',
    title: 'KEY SYSTEMIC ISSUE',
    text: `${topSystemic.join(', ')} menjadi systemic issue utama yang perlu ditangani segera.`,
  },
  { icon: 'target', tone: 'green', title: 'FOCUS AREA', text: configText['Focus Area Note'] },
]

const period = quickFacts.find((f) => f.label === 'Observation Period')?.value ?? '-'
const totalObs = quickFacts.find((f) => f.label === 'Total Observations Completed')?.value ?? '-'

const dashboard = {
  meta: {
    title: 'REGIONAL 4 SIFP ASSURANCE DASHBOARD',
    subtitle: 'Executive Dashboard – Full Database (July 2026)',
    draft: true,
    sourceFile: basename(srcFile),
    generatedAt: new Date().toISOString(),
  },
  kpis: [
    {
      code: 'PSEC',
      title: measures.PSEC.name,
      value: measures.PSEC.value,
      pending: false,
      desc: `${measures.PSEC.numerator} of ${measures.PSEC.denominator} Priority SIF Exposures Verified`,
      variant: 'green',
      target: `TARGET: ${measures.PSEC.target}%`,
    },
    {
      code: 'CCVC',
      title: measures.CCVC.name,
      value: measures.CCVC.value,
      pending: false,
      desc: `${measures.CCVC.numerator} of ${measures.CCVC.denominator} Critical Safeguards Applicable Verified`,
      variant: 'blue',
      target: `TARGET: ${measures.CCVC.target}%`,
    },
    {
      code: 'PSIE',
      title: measures.PSIE.name,
      value: measures.PSIE.value ?? 0,
      pending: measures.PSIE.status === 'Pending',
      desc: measures.PSIE.notes,
      variant: 'purple',
      target: `TARGET: ${measures.PSIE.target}%`,
    },
  ],
  conformance: {
    value: measures.CONF.value,
    target: `TARGET: ${measures.CONF.target}%`,
    bands: [
      { status: 'failed', from: 0, to: 50 },
      { status: 'degraded', from: 50, to: 80 },
      { status: 'effective', from: 80, to: 100 },
    ],
  },
  quickFacts,
  healthMap: { zones: ['Z11', 'Z12', 'Z13', 'Z14'], rows: healthRows },
  topPanels,
  trend: {
    target: measures.CONF.target,
    targetLabel: `Target: ${measures.CONF.target}%`,
    points: trendActual,
    projection: trendProjection,
  },
  zonaScores: {
    target: measures.CONF.target,
    targetLabel: `Target: ${measures.CONF.target}%`,
    bars: zonaScores,
  },
  initiatives: initiatives.map((r) => ({
    name: r.initiative,
    owner: r.owner,
    status: r.status,
    progress: r.progress ?? 0,
  })),
  summaryCards,
  summaryNotes: [
    `Data berdasarkan ${totalObs} observasi pada periode ${period}.`,
    `Data cutoff: ${configText['Data Cutoff'] ?? '-'}.`,
    'Dashboard diperbarui setiap bulan.',
  ],
  footerNote:
    'Dashboard ini menggunakan data observasi V&V (Full Database). Nilai indikator diperbarui otomatis dari hasil konversi Excel setiap kali data observasi bertambah dan tervalidasi.',
}

// ---------- Tulis output ----------

mkdirSync(outDir, { recursive: true })
const outputs = {
  'observations.json': observations,
  'sif-questions.json': sifQuestions,
  'ccvc-library.json': ccvcLibrary,
  'error-traps.json': errorTraps,
  'hp-tools.json': hpTools,
  'drift-conditions.json': driftConditions,
  'latent-conditions.json': latentConditions,
  'initiatives.json': initiatives,
  'dashboard.json': dashboard,
}

for (const [file, data] of Object.entries(outputs)) {
  writeFileSync(join(outDir, file), JSON.stringify(data, null, 2))
  const count = Array.isArray(data) ? data.length : Object.keys(data).length
  console.log(`✓ ${file} (${Array.isArray(data) ? `${count} baris` : `${count} bagian`})`)
}

// ---------- Manifest + data mentah SEMUA worksheet (untuk viewer generik) ----------
// Setiap sheet Excel diekspor apa adanya ke sheets/<slug>.json, dan _manifest.json
// menjadi sumber tunggal daftar menu sidebar -> jumlah menu selalu mengikuti
// jumlah worksheet di file Excel tanpa perlu mengubah kode.

// Sheet yang punya halaman kurasi khusus: arahkan menu ke halaman itu, bukan viewer generik.
const CURATED = {
  'INPUT-SIF_Questions': { route: '/master/sif-questions', label: 'SIF Questions', icon: 'checklist' },
  'INPUT-Error_Traps': { route: '/master/error-traps', label: 'Error Traps', icon: 'warning' },
  'INPUT-HP_Tools': { route: '/master/hp-tools', label: 'HP Tools', icon: 'gear' },
  'INPUT-Drift_Conditions': { route: '/master/drift-conditions', label: 'Drift Conditions', icon: 'refresh' },
  'INPUT-Latent_Conditions': { route: '/master/latent-conditions', label: 'Latent Conditions', icon: 'layers' },
  DATABASE_PSEC_CCVC: { route: '/master/ccvc-library', label: 'PSEC & CCVC Library', icon: 'book' },
  'ANALYZE-CONFORMANCE_SCORE': { route: '/master/observations', label: 'Observations', icon: 'clipboard' },
  'ANALYZE-IMPROVEMENT_INITIATIVES': { route: '/master/initiatives', label: 'Improvement Initiatives', icon: 'target' },
}

// Urutan tampil grup di sidebar; sheet di dalam grup mengikuti urutan aslinya di Excel.
const GROUP_ORDER = ['Data Input', 'Database', 'Analisis', 'Konfigurasi', 'Sumber', 'Audit', 'Helper', 'Lainnya']
const GROUP_ICON = {
  'Data Input': 'clipboard',
  Database: 'book',
  Analisis: 'gear',
  Konfigurasi: 'gear',
  Sumber: 'file',
  Audit: 'shield',
  Helper: 'layers',
  Lainnya: 'file',
}

function groupOf(name) {
  if (/^INPUT/i.test(name)) return 'Data Input'
  if (/^DATABASE/i.test(name)) return 'Database'
  if (/^ANALYZE/i.test(name)) return 'Analisis'
  if (/^CONFIG/i.test(name)) return 'Konfigurasi'
  if (/^SOURCE/i.test(name)) return 'Sumber'
  if (/^AUDIT/i.test(name)) return 'Audit'
  if (/^Helper/i.test(name)) return 'Helper'
  return 'Lainnya'
}

function slugify(name) {
  return name
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

// Label ringkas untuk sheet non-kurasi: buang token kategori di depan (sudah jadi
// judul grup) lalu ubah pemisah "_" / "-" menjadi spasi.
function shortLabel(name) {
  const stripped = name.replace(/^(INPUT|DATABASE|ANALYZE|CONFIG|SOURCE|AUDIT|Helper)[-_]?/i, '')
  const words = (stripped || name).replace(/[_-]+/g, ' ').trim()
  return words || name
}

// raw:false -> tanggal & persen tampil seperti di Excel, bukan angka serial.
function rawGrid(sheetName) {
  const grid = XLSX.utils.sheet_to_json(wb.Sheets[sheetName], {
    header: 1,
    raw: false,
    defval: '',
    blankrows: false,
  })
  const colCount = grid.reduce((m, r) => Math.max(m, r.length), 0)
  const rows = grid.map((r) => Array.from({ length: colCount }, (_, c) => r[c] ?? ''))
  return { rows, colCount }
}

// Hanya sheet yang dipakai converter (feed dashboard & halaman master) yang masuk
// sidebar. Sheet tambahan (Helper_*, AUDIT-*, SOURCE, ReadMe, dsb.) hanya bantu
// internal Excel dan tidak dipakai aplikasi, jadi tidak diikutkan.
const usedSheets = new Set(REQUIRED_SHEET_NAMES)

const sheetsDir = join(outDir, 'sheets')
// Bersihkan dulu supaya file sheet lama (mis. sheet tambahan hasil run sebelumnya)
// tidak tertinggal sebagai berkas mati.
rmSync(sheetsDir, { recursive: true, force: true })
mkdirSync(sheetsDir, { recursive: true })

const usedSlugs = new Set()
const manifestItems = wb.SheetNames.filter((name) => usedSheets.has(name)).map((name, index) => {
  let slug = slugify(name)
  while (usedSlugs.has(slug)) slug += '-x'
  usedSlugs.add(slug)

  const { rows, colCount } = rawGrid(name)
  const curated = CURATED[name]
  const group = groupOf(name)

  writeFileSync(
    join(sheetsDir, `${slug}.json`),
    JSON.stringify({ name, slug, rowCount: rows.length, colCount, rows }, null, 2)
  )

  return {
    name,
    slug,
    index,
    group,
    label: curated?.label ?? shortLabel(name),
    icon: curated?.icon ?? GROUP_ICON[group],
    route: curated?.route ?? `/sheet/${slug}`,
    curated: Boolean(curated),
    rowCount: rows.length,
    colCount,
    dataRows: Math.max(0, rows.length - 1),
  }
})

const sheetGroups = GROUP_ORDER.map((label) => ({
  label,
  items: manifestItems.filter((it) => it.group === label),
})).filter((g) => g.items.length)

writeFileSync(
  join(sheetsDir, '_manifest.json'),
  JSON.stringify(
    {
      generatedAt: new Date().toISOString(),
      sourceFile: basename(srcFile),
      sheetCount: manifestItems.length,
      groups: sheetGroups,
    },
    null,
    2
  )
)
console.log(`✓ sheets/_manifest.json (${manifestItems.length} worksheet, ${sheetGroups.length} grup)`)

console.log(`\nSumber : ${basename(srcFile)}`)
console.log(`Output : ${outDir}`)
