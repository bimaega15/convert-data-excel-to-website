# Referensi API

Base URL pengembangan: `http://localhost:5250`
Dokumentasi interaktif: `http://localhost:5250/swagger`

## Bentuk respons

Seluruh endpoint `/api` memakai amplop yang sama, termasuk saat gagal:

```json
{
  "status": "SUCCESS",
  "message": "OK",
  "data": { },
  "errors": null
}
```

`status` bernilai `SUCCESS` atau `ERROR`. Saat validasi gagal, `errors` berisi
pesan per field:

```json
{
  "status": "ERROR",
  "message": "Data yang dikirim tidak valid.",
  "data": null,
  "errors": {
    "ObsCode": ["Obs ID wajib diisi."],
    "Zona": ["Zona harus antara 1 dan 99."]
  }
}
```

Endpoint berhalaman membungkus hasilnya di `data`:

```json
{
  "status": "SUCCESS",
  "data": {
    "items": [ ],
    "page": 1,
    "pageSize": 25,
    "totalItems": 173,
    "totalPages": 7,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

## Parameter query standar

Berlaku untuk semua endpoint list:

| Parameter | Bawaan | Keterangan |
| --- | --- | --- |
| `page` | 1 | Nomor halaman |
| `pageSize` | 25 | Dibatasi maksimum 200 |
| `search` | – | Pencarian bebas; kolom yang dicari berbeda per endpoint |
| `sortBy` | – | Nama kolom yang diizinkan (hanya di `/api/observations`) |
| `sortDescending` | false | Arah urutan |

Filter tambahan untuk master data: `obsCode`, `zona`, `protocolCode`, `status`, `isActive`.

## Autentikasi

Seluruh endpoint `/api` **terbuka tanpa autentikasi**, baca maupun tulis. Klien Vue
tidak punya halaman login; pembatasan akses direncanakan lewat Windows Authentication
di IIS perusahaan, sehingga tidak diduplikasi di level aplikasi.

Konsekuensinya `POST /api/import/excel` — yang mengganti seluruh master data — juga
bisa dipanggil siapa pun yang dapat menjangkau server. Batasi di level jaringan/IIS.

Satu-satunya pengecualian adalah `/api/users`: masih memerlukan cookie login area
admin (`/admin/login`) dengan role `Administrator`, karena isinya data akun.

---

## Dashboard

| Method | Endpoint | Akses | Keterangan |
| --- | --- | --- | --- |
| GET | `/api/dashboard` | terbuka | Payload dashboard lengkap |
| GET | `/api/dashboard/summary` | terbuka | Ringkasan angka untuk kartu admin |

`GET /api/dashboard` mengembalikan struktur yang **sama persis** dengan
`sifp_vue.client/src/data/generated/dashboard.json`:

```
data.meta          { title, subtitle, draft, sourceFile, generatedAt }
data.kpis[]        { code, title, value, pending, desc, variant, target }
data.conformance   { value, target, bands[] }
data.quickFacts[]  { icon, label, value }
data.healthMap     { zones[], rows[{ name, cells[{status,score,value}], regional, regionalStatus }] }
data.topPanels[]   { no, title, subtitle, variant, dash, items[{label,display,weight}], footer }
data.trend         { target, targetLabel, points[], projection[] }
data.zonaScores    { target, targetLabel, bars[{zone,obs,value}] }
data.initiatives[] { name, owner, status, progress }
data.summaryCards[]{ icon, tone, title, text }
data.summaryNotes[]
data.footerNote
```

---

## Observations

| Method | Endpoint | Akses | Keterangan |
| --- | --- | --- | --- |
| GET | `/api/observations` | terbuka | Daftar berhalaman + filter |
| GET | `/api/observations/all` | terbuka | Semua baris, bentuk = `observations.json` |
| GET | `/api/observations/{id}` | terbuka | Satu observasi |
| GET | `/api/observations/code/{obsCode}` | terbuka | Cari berdasarkan `OBS-001` |
| GET | `/api/observations/{id}/detail` | terbuka | Observasi + seluruh data turunannya |
| GET | `/api/observations/filter-options` | terbuka | Nilai unik untuk dropdown filter |
| POST | `/api/observations` | terbuka | Tambah |
| PUT | `/api/observations/{id}` | terbuka | Ubah |
| DELETE | `/api/observations/{id}` | terbuka | Hapus (beserta data turunannya) |

Filter khusus: `dateFrom`, `dateTo`, `site`, `company`.
`sortBy` menerima: `date`, `zona`, `performance`, `site`, `protocol`.

Bentuk satu observasi:

```json
{
  "id": "OBS-001",
  "key": 47,
  "protocolCode": "WAH",
  "protocolName": "Work at Height",
  "date": "2026-05-14",
  "zona": 13,
  "site": "CPP MATINDOK",
  "area": "GTG Area",
  "activity": "Scaffolding Dismantling",
  "company": "PT Petro Teknik Konstruksi",
  "observers": ["Iman Sudirman", "Endong Darwilisan Yahya", "Wayan Edi Wijaya"],
  "yes": 4, "no": 5, "na": 0,
  "performance": 44.44,
  "sequence": 1,
  "psieEligible": "N",
  "status": "Baseline",
  "active": "Y"
}
```

`id` adalah Obs_ID dari Excel; `key` adalah primary key database yang dipakai untuk
operasi ubah/hapus. Field `psieEligible` dan `active` tetap berbentuk `"Y"`/`"N"`
mengikuti workbook aslinya.

---

## Master Data (read-only)

Diperbarui lewat import Excel, bukan input manual.

| Method | Endpoint | Keterangan |
| --- | --- | --- |
| GET | `/api/master/sif-questions` | + filter `answer` (YES/NO/NA), `ccvcId` |
| GET | `/api/master/error-traps` | |
| GET | `/api/master/hp-tools` | |
| GET | `/api/master/drift-conditions` | |
| GET | `/api/master/latent-conditions` | |
| GET | `/api/master/ccvc-library` | + filter `psecId`, `protocolGroup`, `exposureType` |
| GET | `/api/master/ccvc-library/{ccvcId}` | Satu entri, mis. `CLSR01-A` |
| GET | `/api/master/counts` | Jumlah baris per tabel |

Semuanya terbuka tanpa autentikasi.

```bash
curl "http://localhost:5250/api/master/sif-questions?obsCode=OBS-001&answer=NO"
```

---

## Improvement Initiatives

| Method | Endpoint | Akses |
| --- | --- | --- |
| GET | `/api/initiatives` | terbuka |
| GET | `/api/initiatives/all` | terbuka |
| GET | `/api/initiatives/{id}` | terbuka |
| POST | `/api/initiatives` | terbuka |
| PUT | `/api/initiatives/{id}` | terbuka |
| DELETE | `/api/initiatives/{id}` | terbuka |

---

## Worksheets

Menggantikan `src/data/generated/sheets/*.json`.

| Method | Endpoint | Akses | Keterangan |
| --- | --- | --- | --- |
| GET | `/api/worksheets/manifest` | terbuka | Daftar worksheet dari import terakhir |
| GET | `/api/worksheets/{slug}` | terbuka | Isi mentah satu worksheet |

Manifest sudah dikelompokkan sesuai urutan grup sidebar (Data Input, Database,
Analisis, Konfigurasi, …), jadi jumlah menu di Vue otomatis mengikuti isi workbook:

```json
{
  "generatedAt": "2026-07-31T16:55:00Z",
  "sourceFile": "VnV_FULL_DATABASE_09July2026_OBS001-023 ver.Jul2026.xlsx",
  "sheetCount": 14,
  "groups": [
    {
      "label": "Data Input",
      "items": [
        {
          "name": "INPUT-SIF_Questions", "slug": "input-sif-questions", "index": 0,
          "group": "Data Input", "label": "SIF Questions", "icon": "checklist",
          "route": "/master/sif-questions", "curated": true,
          "rowCount": 174, "colCount": 17, "dataRows": 173
        }
      ]
    }
  ]
}
```

`GET /api/worksheets/{slug}` mengembalikan `{ name, slug, rowCount, colCount, rows }`
dengan `rows` berupa array of array string — baris ke-0 adalah header.

---

## Import

| Method | Endpoint | Akses | Keterangan |
| --- | --- | --- | --- |
| POST | `/api/import/excel` | terbuka | Unggah & proses workbook |
| GET | `/api/import/batches` | terbuka | Riwayat import (berhalaman) |
| GET | `/api/import/batches/{id}` | terbuka | Detail satu batch |
| GET | `/api/import/required-sheets` | terbuka | 14 sheet wajib beserta labelnya |

`POST /api/import/excel` menerima `multipart/form-data` — kontraknya sama dengan
`submitWorkbook()` di `src/services/excelImport.js`:

| Field | Tipe | Keterangan |
| --- | --- | --- |
| `file` | file | Workbook `.xlsx` / `.xlsm`, maks 25 MB |
| `summary` | string (JSON) | Ringkasan hasil parse di klien, untuk audit |
| `edits` | string (JSON) | Daftar perubahan sel dari layar preview |

Bentuk `edits` (penomoran baris/kolom mengikuti Excel, 1-based):

```json
[{ "sheet": "INPUT-SIF_Questions", "excelRow": 7, "excelCol": 3, "cell": "C7", "from": "YES", "to": "NO" }]
```

Respons sukses:

```json
{
  "status": "SUCCESS",
  "message": "Import berhasil.",
  "data": {
    "batchId": 5,
    "fileName": "VnV_FULL_DATABASE_09July2026_OBS001-023 ver.Jul2026.xlsx",
    "status": "Completed",
    "sheetCount": 14,
    "editsApplied": 0,
    "rowsImported": {
      "Observations": 23, "SifQuestions": 173, "ErrorTraps": 56, "HpTools": 39,
      "DriftConditions": 139, "LatentConditions": 156, "CcvcLibraryItems": 133,
      "ImprovementInitiatives": 8, "ExecutiveMeasures": 4, "QuickFacts": 8,
      "ClsrHealthMapRows": 10, "TopFiveItems": 20, "DashboardTexts": 5,
      "TrendPoints": 8, "ZonaScores": 4, "Worksheets": 14
    },
    "warnings": []
  }
}
```

`warnings` memuat masalah non-fatal, misalnya baris yang menunjuk `Obs_ID` yang tidak
ada di sheet `ANALYZE-CONFORMANCE_SCORE` — baris itu dilewati, sisanya tetap diproses.

Bila gagal, transaksi di-rollback sehingga **data lama tetap utuh**:

```json
{
  "status": "ERROR",
  "message": "Import gagal: 2 sheet wajib tidak ditemukan: ANALYZE-TOP5 (Top 5 …), CONFIG-DASHBOARD_TEXT (Teks naratif dashboard)"
}
```

---

## Users

Satu-satunya grup endpoint `/api` yang masih tertutup: memerlukan cookie login
area admin (`/admin/login`) dengan role `Administrator`.

| Method | Endpoint |
| --- | --- |
| GET | `/api/users` |
| GET | `/api/users/{id}` |
| GET | `/api/users/roles` |
| POST | `/api/users` |
| PUT | `/api/users/{id}` |
| DELETE | `/api/users/{id}` |

Menghapus akun yang sedang dipakai ditolak, agar administrator terakhir tidak
mengunci dirinya sendiri di luar sistem.

---

## Ringkasan kode status

| Kode | Arti |
| --- | --- |
| 200 | Berhasil |
| 201 | Dibuat (POST) — header `Location` menunjuk resource baru |
| 400 | Validasi gagal atau aturan bisnis dilanggar (mis. Obs ID duplikat) |
| 401 | Belum login cookie admin (hanya `/api/users`) |
| 403 | Sudah login tetapi role bukan `Administrator` (hanya `/api/users`) |
| 404 | Resource tidak ditemukan |
