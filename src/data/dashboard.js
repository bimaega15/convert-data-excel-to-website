// Seluruh data dashboard ditranskripsi dari design/dashboard-design.jpeg (Pilot Phase, May 2026).
// Nantinya sumber data ini bisa diganti dengan hasil konversi Excel.

export const meta = {
  title: 'REGIONAL 4 SIFP ASSURANCE DASHBOARD',
  subtitle: 'Executive Dashboard – Pilot Implementation (May 2026)',
  draft: true,
}

export const kpis = [
  {
    code: 'PSEC',
    title: 'Priority SIF Exposure Coverage',
    value: 30.0,
    desc: '3 of 10 Priority SIF Exposures Verified',
    variant: 'green',
    target: 'TARGET TW I 2027: ≥ 90%',
  },
  {
    code: 'CCVC',
    title: 'Critical Control Verification Coverage',
    value: 10.0,
    desc: 'Critical Controls Verified Against Annual Target',
    variant: 'blue',
    target: 'TARGET TW I 2027: ≥ 85%',
  },
  {
    code: 'PSIE',
    title: 'Priority SIF Improvement Effectiveness',
    value: 12.5,
    desc: '1 of 8 Improvement Initiatives in Progress',
    variant: 'purple',
    target: 'TARGET TW I 2027: ≥ 80%',
  },
]

export const conformance = {
  value: 50.56,
  target: 'TARGET TW I 2027: ≥ 80%',
  // batas pita status: <50 failed, 50–<80 degraded, ≥80 effective
  bands: [
    { status: 'failed', from: 0, to: 50 },
    { status: 'degraded', from: 50, to: 80 },
    { status: 'effective', from: 80, to: 100 },
  ],
}

export const quickFacts = [
  { icon: 'clipboard', label: 'Total Observations Completed', value: '5' },
  { icon: 'shield', label: 'SIF Exposures Verified', value: '3' },
  { icon: 'checklist', label: 'Critical Safeguards Verified (TEs)', value: '16' },
  { icon: 'calendar', label: 'Observation Period', value: '14 – 21 May 2026' },
  { icon: 'pin', label: 'Sites / Locations (Zona 11, Zona 13)', value: '2' },
  { icon: 'person', label: 'Observers', value: '3' },
]

// status: 'effective' | 'degraded' | 'failed' | 'nodata'
export const healthMap = {
  zones: ['Z11', 'Z12', 'Z13', 'Z14'],
  rows: [
    {
      name: 'Tools & Equipment',
      cells: [
        { status: 'effective' },
        { status: 'effective' },
        { status: 'effective' },
        { status: 'effective' },
      ],
      regional: null,
    },
    {
      name: 'Line of Fire',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'effective' },
        { status: 'effective' },
      ],
      regional: null,
    },
    {
      name: 'Hot Work',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
      ],
      regional: null,
    },
    {
      name: 'Confined Space',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'degraded' },
        { status: 'nodata' },
      ],
      regional: null,
    },
    {
      name: 'Powered System (IHE)',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'degraded' },
        { status: 'nodata' },
      ],
      regional: 62.5,
    },
    {
      name: 'Lifting Operation',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
      ],
      regional: null,
    },
    {
      name: 'Working at Height',
      cells: [
        { status: 'failed', value: 33.33 },
        { status: 'nodata' },
        { status: 'failed', value: 44.44 },
        { status: 'nodata' },
      ],
      regional: 38.89,
    },
    {
      name: 'Ground-Disturbance Work',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'degraded' },
        { status: 'nodata' },
      ],
      regional: 50.0,
    },
    {
      name: 'Water-Based Work Activities',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
      ],
      regional: null,
    },
    {
      name: 'Land Transportation',
      cells: [
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
        { status: 'nodata' },
      ],
      regional: null,
    },
  ],
}

export const statusLegend = [
  { status: 'effective', label: 'Effective (≥80%)' },
  { status: 'degraded', label: 'Degraded (50% – <80%)' },
  { status: 'failed', label: 'Failed / High Concern (<50%)' },
  { status: 'nodata', label: 'No Data' },
]

export const topPanels = [
  {
    no: 2,
    title: 'TOP 5 SIF EXPOSURES',
    subtitle: '(by Frequency)',
    variant: 'green',
    dash: 'green',
    items: [
      { label: 'Isolation Hazardous Energy (IHE)', display: '2 (40%)', weight: 1 },
      { label: 'Work at Height (WAH)', display: '2 (40%)', weight: 1 },
      { label: 'Ground Disturbance (GD)', display: '1 (20%)', weight: 0.5 },
      { label: 'Others', display: '0 (0%)', weight: 0 },
      { label: '-', display: '0 (0%)', weight: 0 },
    ],
    footer: { icon: 'clipboard', label: 'Total Observations', value: 5 },
  },
  {
    no: 3,
    title: 'TOP 5 CRITICAL SAFEGUARD GAPS',
    subtitle: '(by Frequency)',
    variant: 'red',
    dash: 'red',
    items: [
      { label: 'Rescue Plan', display: '1', weight: 1 },
      { label: 'Permit / Authorization', display: '1', weight: 1 },
      { label: 'Isolation Verification', display: '1', weight: 1 },
      { label: 'Anchorage Verification', display: '1', weight: 1 },
      { label: 'LOTO – Verification', display: '1', weight: 1 },
    ],
    footer: { icon: 'shield', label: 'Total Findings', value: 5 },
  },
  {
    no: 4,
    title: 'TOP 5 RECURRING DRIFT',
    subtitle: '(Observed)',
    variant: 'blue',
    dash: 'amber',
    items: [
      { label: 'Verification not done', display: '1', weight: 1 },
      { label: 'Permit expired / not updated', display: '1', weight: 1 },
      { label: 'Inadequate documentation', display: '1', weight: 1 },
      { label: 'Communication gap', display: '1', weight: 1 },
      { label: '-', display: '0', weight: 0 },
    ],
    footer: { icon: 'refresh', label: 'Total Occurrences', value: 4 },
  },
  {
    no: 5,
    title: 'TOP 5 SYSTEMIC ISSUES',
    subtitle: '(Identified)',
    variant: 'purple',
    dash: null,
    items: [
      { label: 'Competency & Training', display: '1', weight: 1 },
      { label: 'Work Planning & Preparation', display: '1', weight: 1 },
      { label: 'Leadership & Supervision', display: '1', weight: 1 },
      { label: 'Procedure & Documentation', display: '1', weight: 1 },
      { label: '-', display: '0', weight: 0 },
    ],
    footer: { icon: 'gear', label: 'Total Findings', value: 4 },
  },
]

export const trend = {
  target: 80,
  targetLabel: 'Target TW I 2027: ≥ 80%',
  points: [
    { month: 'Jan-26', value: 22.1 },
    { month: 'Feb-26', value: 28.4 },
    { month: 'Mar-26', value: 32.2 },
    { month: 'Apr-26', value: 41.3 },
    { month: 'May-26', value: 50.56 },
  ],
}

export const zonaScores = {
  target: 80,
  targetLabel: 'Target TW I 2027: ≥ 80%',
  bars: [
    { zone: 'Zona 11', obs: 1, value: 33.33 },
    { zone: 'Zona 13', obs: 4, value: 52.48 },
    { zone: 'Zona 12', obs: 0, value: 0 },
    { zone: 'Zona 14', obs: 0, value: 0 },
  ],
}

export const initiatives = [
  {
    name: 'AI-assisted Risk Assessment and JSA',
    owner: 'HSSE Zona 14',
    status: 'In Progress',
    progress: 20,
  },
  {
    name: 'CLSR #5 Powered System: Individual LOTO campaign',
    owner: 'HSSE Zona 13',
    status: 'In Progress',
    progress: 15,
  },
  {
    name: 'CLSR #7 WAH: Develop SOP for erection and dismantling scaffolding together with BP',
    owner: 'HSSE Zona 11',
    status: 'In Progress',
    progress: 5,
  },
  {
    name: 'CLSR #7 WAH: Hierarchy of WAH campaign',
    owner: 'HSSE Zona 12',
    status: 'In Progress',
    progress: 7,
  },
  {
    name: 'CLSR #8 Ground disturbance: SWC improvement training',
    owner: 'HSSE Zona 14',
    status: 'In Progress',
    progress: 5,
  },
  {
    name: 'Business partner (BP) V&V coaching',
    owner: 'HSSE Zona 14',
    status: 'In Progress',
    progress: 10,
  },
]

export const summaryCards = [
  {
    icon: 'warning',
    tone: 'red',
    title: 'TOP SIF EXPOSURES',
    text: 'Work at Height (WAH) dan Isolation Hazardous Energy (IHE) merupakan SIF exposures paling sering diobservasi.',
  },
  {
    icon: 'shield',
    tone: 'navy',
    title: 'CRITICAL GAPS',
    text: 'Rescue Plan, Permit, Isolation Verification, Anchorage Verification dan LOTO Verification perlu menjadi fokus perbaikan.',
  },
  {
    icon: 'pin',
    tone: 'red',
    title: 'ZONA ATTENTION',
    text: 'Zona 11 menunjukkan performance rendah (33.33%). Perlu penguatan kontrol dan compliance di area tersebut.',
  },
  {
    icon: 'people',
    tone: 'navy',
    title: 'KEY SYSTEMIC ISSUE',
    text: 'Kompetensi, Work Planning & Preparation, serta Leadership menjadi sistemik issue utama yang perlu ditangani segera.',
  },
  {
    icon: 'target',
    tone: 'green',
    title: 'FOCUS AREA',
    text: 'Perkuat verifikasi critical safeguards, perbaiki proses perizinan, dan tingkatkan kompetensi personel.',
  },
]

export const summaryNotes = [
  'Data berdasarkan 5 observasi awal pada periode 14 – 21 Mei 2026.',
  'Dashboard diperbarui setiap bulan.',
]

export const footerNote =
  'Dashboard ini menggunakan data observasi awal (Pilot Phase). Nilai indikator akan diperbarui secara otomatis seiring bertambahnya data observasi V&V dan setelah proses validasi.'
