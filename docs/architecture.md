# Arsitektur

## Lapisan

```
Controllers/Api        Controllers/Admin        ← HTTP: JWT vs cookie, JSON vs Razor
        │                      │
        └──────────┬───────────┘
                   ▼
              Services/          ← logika bisnis, aturan validasi, transaksi
                   │               kontraknya di Services/Contracts/
                   ▼
            Repositories/        ← komposisi query, filter, paging
                   │
                   ▼
             SifpDbContext       ← EF Core → SQL Server
```

Aturan yang dipegang konsisten:

- **Controller tidak menyentuh DbContext.** Tugasnya hanya binding, otorisasi, dan
  menerjemahkan hasil service menjadi status HTTP.
- **Service mengembalikan `ApiResponse<T>`**, bukan melempar exception untuk kegagalan
  yang bisa diperkirakan (Obs ID duplikat, data tidak ditemukan). Controller API dan
  controller Razor memakai hasil yang sama, hanya cara menampilkannya berbeda.
- **Repository mengembalikan `IQueryable`** untuk operasi baca, supaya filter dan paging
  tetap dieksekusi di sisi database, bukan di memori.
- **Entity tidak pernah keluar sebagai respons.** Pemetaan ke DTO ditulis manual di
  `Services/Mappers/MasterDataMapper.cs` agar bentuk JSON yang dikonsumsi Vue eksplisit
  dan tidak berubah diam-diam saat properti entity diubah namanya.

Dua controller memakai skema autentikasi berbeda dan itu disengaja:

| | `Controllers/Api` | `Controllers/Admin` |
| --- | --- | --- |
| Basis | `ApiControllerBase` | `AdminBaseController` |
| Skema | JWT Bearer | Cookie |
| Policy | `[Authorize(Roles = …)]` | `AdminOnly` |
| Balasan gagal | JSON `ApiResponse` | redirect + `TempData` |

Cookie handler mengembalikan **401/403** alih-alih redirect HTML bila request berupa
XHR/JSON, supaya kode klien tidak salah menganggap halaman login sebagai keberhasilan.

---

## Skema database

21 tabel dalam empat kelompok.

### Master data observasi

`Observations` adalah induk; lima tabel di bawahnya `ON DELETE CASCADE`.

```
Observations (ObsCode unik)
├── SifQuestions        173 baris   jawaban verifikasi SIF
├── ErrorTraps           56 baris
├── HpTools              39 baris
├── DriftConditions     139 baris
└── LatentConditions    156 baris

CcvcLibraryItems        133 baris   referensi PSEC/CCVC (CcvcId unik)
ImprovementInitiatives    8 baris   (ImprovementCode unik)
```

Kelima tabel anak mengimplementasikan `IObservationChild`, sehingga satu
`ObservationChildRepository<T>` menangani filter "per observasi" untuk semuanya
tanpa menyalin kode lima kali.

### Agregat dashboard

Hasil turunan yang sudah dihitung di dalam workbook, disimpan apa adanya:

| Tabel | Sumber sheet |
| --- | --- |
| `ExecutiveMeasures` | ANALYZE-EXECUTIVE_MEASURES (PSEC, CCVC, PSIE, CONF) |
| `QuickFacts` | ANALYZE-QUICK_FACTS |
| `ClsrHealthMapRows` | ANALYZE-CLSR_HEALTH_MAP |
| `TopFiveItems` | ANALYZE-TOP5 (4 kategori dalam satu tabel) |
| `TrendPoints` | ANALYZE-TREND_ZONE (kolom kiri) |
| `ZonaScores` | ANALYZE-TREND_ZONE (kolom kanan) |
| `DashboardTexts` | CONFIG-DASHBOARD_TEXT |

### Import & worksheet mentah

```
ImportBatches           satu baris per upload; menyimpan hash file, status, edits
└── Worksheets          metadata sheet → sumber menu sidebar Vue
    └── WorksheetRows   isi sel sebagai JSON array
```

`WorksheetRows.CellsJson` disimpan sebagai JSON, bukan kolom terpisah, karena jumlah
kolom berbeda-beda antar sheet — skema database tidak boleh ikut berubah setiap kali
workbook berubah.

### Keamanan

`Users`, `Roles`, `UserRoles` (many-to-many). `Role.CanAccessAdmin` menentukan siapa
yang boleh membuka `/admin`.

### Kolom audit

Semua tabel utama mewarisi `AuditableEntity` (`CreatedAt`, `CreatedBy`, `UpdatedAt`,
`UpdatedBy`). Pengisiannya dipusatkan di `SifpDbContext.SaveChangesAsync`, jadi tidak
ada service yang perlu mengisinya manual. `CreatedAt`/`CreatedBy` dikunci agar tidak
ikut berubah saat update.

### Catatan foreign key

Semua FK ke `ImportBatches` memakai `DeleteBehavior.Restrict`, bukan Cascade/SetNull.
SQL Server menolak dua jalur cascade menuju tabel yang sama, dan tanpa Restrict akan
muncul dua jalur: `ImportBatch → Observation → SifQuestion` dan `ImportBatch → SifQuestion`.
Penghapusan data lama karenanya dilakukan eksplisit oleh `ExcelImportService`.

---

## Alur import Excel

`ExcelImportService` menggantikan `sifp_vue.client/scripts/convert-excel.mjs`. Aturan
pemetaan kolomnya disalin baris demi baris dari converter tersebut, termasuk hal-hal
yang tidak terlihat jelas:

- Sheet `INPUT-Error_Traps` punya header ganda (`Protocols` dua kali) sehingga dibaca
  **per indeks kolom**, bukan per nama.
- `INPUT-Latent_Conditions` menaruh sequence/status/active di indeks 10–12, berbeda
  dari `INPUT-Drift_Conditions` yang memakai 8–10.
- `ANALYZE-TREND_ZONE` memuat dua tabel berdampingan; keduanya dibaca dalam satu lintasan.
- Baris tren dianggap **proyeksi** bila tidak punya nilai aktual tetapi punya angka
  rencana, dan hanya setelah minimal satu bulan realisasi ada.
- Persen di Excel tersimpan sebagai pecahan (0.4444) dan dinaikkan ke skala 0–100.
- Angka di sel `Fact_Value` yang ≤ 1 selalu berupa rasio, jadi ditampilkan sebagai persen.

Urutan eksekusi:

```
1. Validasi ekstensi & ukuran
2. Salin stream ke memori, hitung SHA-256
3. Simpan ImportBatch (status Processing)
4. Buka workbook (ClosedXML), terapkan `edits` dari layar preview
5. Verifikasi 14 sheet wajib  ── gagal di sini → tidak ada data yang tersentuh
6. ┌ execution strategy + transaksi ──────────────────┐
   │ hapus seluruh master data lama                   │
   │ parse & simpan Observations (dapatkan Id-nya)    │
   │ parse & simpan tabel anak, katalog, agregat      │
   │ simpan Worksheets + WorksheetRows                │
   │ tandai batch Completed                           │
   └ commit ──────────────────────────────────────────┘
7. Gagal → rollback (data lama utuh) + batch ditandai Failed beserta pesannya
```

Langkah 6 dibungkus `CreateExecutionStrategy()` karena `EnableRetryOnFailure` pada
provider SQL Server melarang transaksi yang dibuka manual kecuali seluruh operasinya
membentuk satu unit yang bisa diulang. Delegasi di dalamnya karena itu ditulis agar
aman dijalankan ulang: change tracker dan daftar warning dibersihkan di awal.

Baris yang menunjuk `Obs_ID` tidak dikenal **dilewati dengan peringatan**, bukan
menggagalkan seluruh import — satu baris cacat di workbook tidak sebanding dengan
membatalkan 700+ baris lainnya.

### Kesetaraan dengan converter JavaScript

Diverifikasi dengan mengimport workbook asli lalu membandingkan hasilnya terhadap
`src/data/generated/*.json`:

| | Converter JS | Import server |
| --- | --- | --- |
| Observations | 23 | 23 |
| SifQuestions | 173 | 173 |
| ErrorTraps | 56 | 56 |
| HpTools | 39 | 39 |
| DriftConditions | 139 | 139 |
| LatentConditions | 156 | 156 |
| CcvcLibraryItems | 133 | 133 |
| Initiatives | 8 | 8 |
| Worksheets | 14 | 14 |
| Trend (aktual/proyeksi) | 3 / 5 | 3 / 5 |

`GET /api/dashboard` juga dibandingkan field demi field terhadap `dashboard.json`:
seluruh KPI, conformance, quick facts, health map, top panel, tren, skor zona,
kartu ringkasan, dan catatan cocok nilai-per-nilai.

---

## Menyusun ulang dashboard

`DashboardService` membangun payload dashboard dari tabel-tabel agregat. Aturan
turunannya juga disalin dari converter:

- Pemetaan status Excel → kelas CSS: `Effective` → `effective`,
  `Failed / High Concern` → `failed`, dan seterusnya.
- Angka hanya dicetak di dalam sel health map berstatus `failed` (mengikuti desain).
- `weight` tiap item Top 5 adalah rasio terhadap item terbesar di panelnya;
  pembaginya dijaga minimal 1 supaya panel kosong tidak memicu pembagian nol.
- Varian warna kartu KPI ditetapkan per metrik: PSEC hijau, CCVC biru, PSIE ungu.

Semua angka yang disisipkan ke dalam string (`"51.88%"`, `"8 (35%)"`, `"TARGET: 100%"`)
diformat dengan **InvariantCulture**. Tanpa itu, server dengan locale Indonesia
menghasilkan `51,88%` dan klien membacanya sebagai angka yang salah — kelas bug yang
hanya muncul di mesin non-US, dan sempat terjadi selama pengembangan ini.

---

## Kesetaraan bentuk data dengan klien

DTO master data sengaja dibuat identik dengan file JSON hasil konversi, termasuk
detail yang terlihat aneh dari sisi C#:

- `psieEligible` dan `active` berupa `"Y"`/`"N"`, bukan boolean.
- `id` berisi Obs_ID (`"OBS-001"`), sedangkan primary key ada di `key`.
- `date` berupa string ISO `"2026-05-14"`, bukan `DateTime`.

Berkat itu, perpindahan halaman Vue dari JSON statis ke API tidak menyentuh satu
pun template komponen — hanya sumber datanya yang berganti.

Di dalam database, nilai-nilai itu disimpan dalam bentuk yang benar (`bit` untuk
flag, `date` untuk tanggal); konversinya terjadi di lapisan mapper.

---

## Sisi klien (sifp_vue.client)

```
services/api.js          fetch wrapper: base URL, buka amplop ApiResponse, ApiError
services/auth.js         token JWT + profil user, disimpan di localStorage
composables/useApiResource.js   pola loading/error/reload + pembatalan request
components/ui/DataState.vue     panel loading / gagal+retry / kosong
data/dashboard.js        wadah reaktif, diisi loadDashboard()
data/sheets.js           manifest worksheet, diisi loadSheetManifest()
```

Dua keputusan yang menentukan bentuk kode ini:

**Data awal dimuat sebelum `mount()`.** Komponen dashboard membaca datanya secara
sinkron di tingkat setup (`const actual = trend.points`, `zonaScores.bars.length`),
dan sidebar menyusun menunya dari manifest. Kalau data datang setelah render,
komponen-komponen itu memegang snapshot kosong. Karena itu `main.js` menunggu
`loadDashboard()` dan `loadSheetManifest()` selesai lebih dulu — dan sebagai
hasilnya tidak ada satu pun komponen dashboard yang perlu diubah.

**Ekspor `data/dashboard.js` tetap objek/array biasa, bukan `ref`.** Membungkusnya
sebagai `computed` akan menabrak penggunaan yang sudah ada: `conformance.value`
di `ConformanceCard.vue` berarti *properti* `value` (angkanya), sementara pada ref
`.value` berarti isi ref-nya. Wadah reaktif yang diisi di tempat (`Object.assign`,
`splice`) menghindari tabrakan makna itu sepenuhnya.

Halaman master memuat datanya sendiri saat dibuka lewat `useApiResource`, yang juga
membatalkan request sebelumnya agar respons lama tidak menimpa hasil terbaru.
`fetchAllPages()` menarik seluruh baris (tabel master masih ratusan baris, dan
sort/filter dilakukan di klien).

Saat backend mati, `main.js` merender layar penjelasan berisi pesan error dan
perintah untuk menjalankan server — bukan halaman kosong tanpa keterangan.

---

## Yang belum dikerjakan

- Belum ada proyek test otomatis. Verifikasi dilakukan manual terhadap SQL Server
  dan browser sungguhan: migration dari database kosong, seeder, import workbook
  asli, seluruh endpoint, 17 halaman admin, siklus CRUD penuh, serta render
  headless seluruh halaman Vue.
- Refresh token belum ada; token berumur 8 jam dan harus login ulang setelahnya.
- Halaman master mengambil seluruh baris sekaligus. Bila data tumbuh jauh lebih
  besar, paging perlu dipindah ke sisi server (endpoint-nya sudah mendukung).
- Import hanya bisa dari halaman Vue atau `/admin/imports`; belum ada penjadwalan
  atau pemantauan proses import yang berjalan lama.
