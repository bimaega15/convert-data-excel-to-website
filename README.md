# SIFP Assurance — Regional 4

Aplikasi dashboard V&V (Verification & Validation) SIFP Regional 4. Data berasal dari
workbook Excel yang diimport, lalu disimpan di SQL Server dan disajikan lewat REST API
untuk aplikasi Vue serta halaman admin Razor (MVC).

Struktur solusi mengikuti pola **Urbuddy** (`C:\VueJs\Urbuddy`): satu proyek ASP.NET Core
sebagai server, satu proyek Vue sebagai klien, disatukan dalam satu file `.sln`.

```
convert-data-excel-to-website/
├── Sifp_Vue.sln
├── Sifp_Vue.Server/            # ASP.NET Core 8 — MVC + Web API + EF Core
│   ├── Controllers/
│   │   ├── Admin/              # Halaman Razor  (/admin/*, autentikasi cookie)
│   │   └── Api/                # REST API       (/api/*,   autentikasi JWT)
│   ├── Data/
│   │   ├── Configurations/     # Fluent API per entity
│   │   ├── Migrations/         # Migration EF Core
│   │   ├── Seeders/            # Role, user admin, master data awal
│   │   └── SifpDbContext.cs
│   ├── Helpers/                # JWT, password hashing, pembaca Excel, paging
│   ├── Models/
│   │   ├── Entities/           # Model database
│   │   ├── Dtos/               # Kontrak API
│   │   └── ViewModels/         # Model halaman Razor
│   ├── Repositories/           # Akses data (EF Core)
│   ├── Services/               # Logika bisnis + Contracts/
│   ├── Views/Admin/            # Razor views area admin
│   ├── wwwroot/                # CSS/JS admin; target build Vue untuk produksi
│   └── Program.cs
└── sifp_vue.client/            # Vue 3 + Vite (aplikasi yang sudah ada)
    ├── src/  scripts/  design/
    └── package.json
```

Dokumentasi rinci:

| Dokumen | Isi |
| --- | --- |
| [docs/setup.md](docs/setup.md) | Cara menjalankan: SQL Server, migration, seeder, dev server |
| [docs/api.md](docs/api.md) | Daftar endpoint, bentuk respons, autentikasi |
| [docs/architecture.md](docs/architecture.md) | Lapisan aplikasi, skema database, alur import Excel |

---

## Jalan cepat

```bash
# 1. Backend  (butuh .NET 8 SDK + SQL Server)
cd Sifp_Vue.Server
dotnet run
#    -> http://localhost:5250/swagger   API docs
#    -> http://localhost:5250/admin     halaman admin (admin / Admin#12345)

# 2. Frontend  (jalankan di terminal terpisah, backend harus sudah hidup)
cd sifp_vue.client
npm install
npm run dev
#    -> http://localhost:5173   (permintaan /api diproksi ke :5250)
```

Aplikasi Vue mengambil seluruh datanya dari backend, jadi backend perlu berjalan
lebih dulu. Untuk menjalankan semuanya dari satu alamat (seperti di produksi):

```bash
cd sifp_vue.client && npm run build
cp -r dist/* ../Sifp_Vue.Server/wwwroot/
#    -> http://localhost:5250 menyajikan aplikasi Vue sekaligus API-nya
```

Saat pertama dijalankan, server otomatis:

1. menjalankan migration (membuat 21 tabel di database `SifpAssurance`),
2. membuat role `Administrator` / `Verifier` / `Viewer` dan user `admin`,
3. mengisi master data dari `sifp_vue.client/src/data/generated/*.json` — jadi
   dashboard langsung berisi data tanpa perlu upload Excel dulu.

> **Ganti password admin bawaan** (`Admin#12345`) sebelum dipakai di luar mesin
> developer. Lihat [docs/setup.md](docs/setup.md#keamanan).

---

## Dua pintu masuk

| | Aplikasi Vue | Area Admin |
| --- | --- | --- |
| URL | `/` (dev: `localhost:5173`) | `/admin` |
| Teknologi | Vue 3 + Vite | Razor Pages/MVC + Bootstrap 5 |
| Autentikasi | JWT Bearer (`/api/auth/login`) | Cookie (`/admin/login`) |
| Untuk | Dashboard & viewer worksheet | Pengelolaan data, import, user |

Keduanya memakai service dan database yang sama; hanya cara autentikasi dan
cara render-nya yang berbeda.

---

## Alur data

```
workbook Excel (.xlsx)
        │
        │  POST /api/import/excel   (atau form di /admin/imports)
        ▼
ExcelImportService  ── parse ClosedXML → validasi sheet wajib → satu transaksi
        │
        ▼
SQL Server  (Observations, SifQuestions, Worksheets, ExecutiveMeasures, …)
        │
        ├── GET /api/dashboard          → DashboardService menyusun ulang payload dashboard
        ├── GET /api/observations       → master data berhalaman
        ├── GET /api/worksheets/manifest→ menu sidebar Vue (mengikuti isi workbook)
        └── /admin/*                    → halaman Razor untuk pengelolaan
```

`ExcelImportService` menggantikan peran `sifp_vue.client/scripts/convert-excel.mjs`:
aturan pemetaan kolomnya disalin baris demi baris dari converter tersebut, sehingga
hasil di database identik dengan hasil konversi JSON.

**Sudah diverifikasi**: import workbook asli
(`VnV_FULL_DATABASE_09July2026_OBS001-023 ver.Jul2026.xlsx`) menghasilkan jumlah baris
yang sama persis dengan converter JavaScript (23 observasi, 173 SIF question,
139 drift, 156 latent, 133 CCVC, 14 worksheet), dan payload `GET /api/dashboard`
sama nilai-per-nilai dengan `src/data/generated/dashboard.json`.

---

## Vue mengambil data dari API

Seluruh halaman Vue kini membaca datanya dari backend, bukan lagi dari file JSON
statis. Sumber tiap bagian:

| Bagian aplikasi | Endpoint |
| --- | --- |
| Dashboard | `GET /api/dashboard` |
| Menu sidebar (mengikuti isi workbook) | `GET /api/worksheets/manifest` |
| Viewer worksheet `/sheet/:slug` | `GET /api/worksheets/{slug}` |
| Observations | `GET /api/observations/all` |
| SIF Questions / Error Traps / HP Tools / Drift / Latent / CCVC | `GET /api/master/*` |
| Improvement Initiatives | `GET /api/initiatives/all` |
| Import Excel | `POST /api/import/excel` |

Endpoint baca terbuka tanpa token (`[AllowAnonymous]`), jadi dashboard dan tabel
master bisa dibuka tanpa login. Yang memerlukan login hanya import.

Data dashboard dan manifest sidebar diambil **sebelum** app di-mount
(`src/main.js`), karena komponen dashboard dan sidebar membacanya secara sinkron.
Halaman master memuat datanya sendiri saat dibuka, lengkap dengan status
loading / gagal / kosong lewat komponen `DataState`.

Bila backend tidak aktif, aplikasi menampilkan layar penjelasan beserta perintah
untuk menjalankannya — bukan halaman kosong.

> `src/data/generated/*.json` sengaja tetap disimpan: kini perannya hanya sebagai
> data awal untuk `MasterDataSeeder` di backend, bukan lagi sumber data aplikasi.
> `npm run convert:excel` masih berguna untuk menyegarkan berkas seed itu.

### Login

Aplikasi memakai JWT. Halaman `/login` menukar username & password lewat
`POST /api/auth/login`, lalu token disimpan di `localStorage` dan otomatis
dilampirkan pada permintaan yang memerlukannya. Token kedaluwarsa dibuang sendiri
saat aplikasi dimuat, dan respons 401 langsung mengakhiri sesi.

---

## Perintah yang sering dipakai

```bash
# Backend
dotnet build Sifp_Vue.sln
dotnet run   --project Sifp_Vue.Server

# Migration
dotnet ef migrations add <Nama> --project Sifp_Vue.Server --output-dir Data/Migrations
dotnet ef database update      --project Sifp_Vue.Server

# Frontend
cd sifp_vue.client
npm run dev            # dev server
npm run build          # build produksi
npm run convert:excel  # regenerasi JSON statis dari workbook di design/
```
