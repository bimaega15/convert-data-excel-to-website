# Panduan Setup

## Kebutuhan

| Komponen | Versi | Catatan |
| --- | --- | --- |
| .NET SDK | 8.0+ | `dotnet --list-sdks` |
| SQL Server | 2019+ / Express / LocalDB | LocalDB sudah cukup untuk pengembangan |
| Node.js | 18+ | untuk `sifp_vue.client` |
| dotnet-ef | 8.0.13 | `dotnet tool install --global dotnet-ef --version 8.0.13` |

---

## 1. Connection string

Diatur di `Sifp_Vue.Server/appsettings.Development.json` pada
`ConnectionStrings:SifpDatabase`. Nilai bawaannya memakai LocalDB:

```json
"ConnectionStrings": {
  "SifpDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=SifpAssurance;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Sesuaikan `Server=` dengan instance yang Anda pakai:

| Instance | Nilai `Server=` |
| --- | --- |
| LocalDB | `(localdb)\MSSQLLocalDB` |
| SQL Server Express | `.\SQLEXPRESS` |
| Instance default | `.` atau `localhost` |
| Docker / SQL auth | `localhost,1433;User Id=sa;Password=…` |

Database **tidak perlu dibuat manual** — migration akan membuatnya.

---

## 2. Menjalankan backend

```bash
cd Sifp_Vue.Server
dotnet run
```

Saat start, `DatabaseSeeder` menjalankan tiga langkah secara berurutan:

1. **Migration** — membuat/menyelaraskan 21 tabel.
2. **IdentitySeeder** — role `Administrator`, `Verifier`, `Viewer` + user `admin`.
3. **MasterDataSeeder** — mengisi master data dari
   `sifp_vue.client/src/data/generated/*.json`, hanya bila tabel `Observations` masih kosong.

Alamat yang tersedia:

| URL | Isi |
| --- | --- |
| `http://localhost:5250/swagger` | Dokumentasi & uji coba API |
| `http://localhost:5250/admin` | Area admin (Razor) |
| `http://localhost:5250/api/dashboard` | Payload dashboard |

Login admin bawaan: **`admin` / `Admin#12345`**.

### Mematikan seeding

```json
"Seed": {
  "AutoMigrate": true,      // jalankan migration saat start
  "RunSeeders": true,       // jalankan seeder saat start
  "SeedSampleData": false   // lewati master data contoh (role & admin tetap dibuat)
}
```

Untuk produksi, umumnya `AutoMigrate: false` dan migration dijalankan terpisah
lewat pipeline (`dotnet ef database update` atau skrip SQL).

---

## 3. Menjalankan frontend

```bash
cd sifp_vue.client
npm install
npm run dev          # http://localhost:5173
```

**Backend harus sudah berjalan lebih dulu** — aplikasi Vue mengambil seluruh
datanya dari API, tidak lagi dari file JSON statis.

Dev server mem-proxy `/api` ke `http://localhost:5250` (diatur di
`vite.config.js`), sehingga klien dan API berada pada origin yang sama seperti di
produksi. Karena itu tidak ada CORS yang perlu diurus saat pengembangan.

Bila backend memakai port lain, isi `VITE_API_PROXY` di `.env`. Bila frontend
benar-benar di-deploy pada domain terpisah, isi `VITE_API_BASE` dan daftarkan
domainnya pada `Cors:AllowedOrigins` di `appsettings.json`.

### Build produksi

Agar server juga menyajikan aplikasi Vue di `/`:

```bash
cd sifp_vue.client
npm run build
# salin hasil build ke wwwroot server
cp -r dist/* ../Sifp_Vue.Server/wwwroot/
```

`Program.cs` sudah punya fallback: seluruh rute selain `/api`, `/admin`, dan
`/swagger` dilayani dengan `wwwroot/index.html` supaya Vue Router bekerja.

---

## 4. Migration

```bash
# menambah migration baru setelah mengubah entity
dotnet ef migrations add NamaPerubahan \
  --project Sifp_Vue.Server --output-dir Data/Migrations

# menerapkan ke database
dotnet ef database update --project Sifp_Vue.Server

# membatalkan migration terakhir (belum diterapkan)
dotnet ef migrations remove --project Sifp_Vue.Server

# menghasilkan skrip SQL untuk DBA / produksi
dotnet ef migrations script --project Sifp_Vue.Server --idempotent --output deploy.sql
```

> Jalankan `dotnet build` dulu sebelum `database update` bila memakai `--no-build`;
> migration yang belum ikut terkompilasi tidak akan terlihat oleh EF.

Migration awal `InitialCreate` membuat 21 tabel, 48 indeks, dan 10 unique constraint.

---

## 5. Import workbook

Tiga cara, semuanya melewati `ExcelImportService` yang sama:

**a. Halaman admin** — `/admin/imports`, pilih file, klik Import.

**b. REST API** (dipakai halaman Import Excel di Vue):

```bash
curl -X POST http://localhost:5250/api/import/excel \
  -F "file=@workbook.xlsx" \
  -F 'summary={"sheetCount":14}' \
  -F 'edits=[]'
```

**c. Dari aplikasi Vue** — halaman `/import`. Tidak perlu konfigurasi tambahan:
tujuannya sudah otomatis `/api/import/excel`.

Endpoint ini terbuka tanpa login — lihat [Keamanan](#keamanan). Setelah import
berhasil, dashboard dan menu sidebar langsung dimuat ulang mengikuti workbook baru.

Isi `VITE_IMPORT_ENDPOINT` hanya bila workbook perlu dikirim ke layanan lain.

### Yang terjadi saat import

1. Ekstensi dan ukuran file diperiksa (`.xlsx`/`.xlsm`, maks 25 MB).
2. Baris `ImportBatch` dibuat dengan status `Processing` + hash SHA-256 file.
3. Perubahan sel dari layar preview (`edits`) diterapkan ke workbook.
4. Kehadiran 14 sheet wajib diverifikasi — bila kurang, import dibatalkan.
5. **Dalam satu transaksi**: seluruh master data lama dihapus, data baru dimasukkan.
6. Batch ditandai `Completed`; bila gagal, transaksi di-rollback (data lama utuh)
   dan batch ditandai `Failed` beserta pesan errornya.

---

## Keamanan

### Akses aplikasi

Aplikasi Vue **tidak punya halaman login**, dan seluruh endpoint `/api` terbuka —
termasuk `POST /api/import/excel` yang mengganti seluruh master data. Ini disengaja:
pembatasan akses direncanakan lewat **Windows Authentication di IIS perusahaan**.

Sampai Windows Authentication aktif, siapa pun yang bisa menjangkau server juga bisa
menimpa seluruh data. Jangan menerbitkan instance ini ke jaringan yang lebih luas dari
yang dimaksud; batasi di level jaringan atau IIS.

Area `/admin` tetap memakai login cookie sendiri (`/admin/login`), begitu pula
`GET /api/users` yang mengikutinya.

### Rahasia konfigurasi

Nilai berikut **wajib** diganti sebelum keluar dari mesin developer:

| Setting | Cara aman |
| --- | --- |
| `Seed:AdminPassword` | password admin awal |
| `ConnectionStrings:SifpDatabase` | kredensial database |

Gunakan user-secrets saat pengembangan:

```bash
cd Sifp_Vue.Server
dotnet user-secrets init
dotnet user-secrets set "Seed:AdminPassword" "PasswordKuatAnda"
dotnet user-secrets set "ConnectionStrings:SifpDatabase" "Server=…"
```

Atau environment variable saat deploy (pemisahnya dua garis bawah):

```
Seed__AdminPassword=…
ConnectionStrings__SifpDatabase=…
```

Password disimpan sebagai PBKDF2-HMAC-SHA256, 210.000 iterasi, salt 16 byte acak,
berformat `iterations.salt.hash` sehingga hash lama tetap terverifikasi ketika
parameter iterasinya dinaikkan.

---

## Masalah yang sering muncul

| Gejala | Penyebab & solusi |
| --- | --- |
| `Connection string 'SifpDatabase' belum diatur` | Isi `ConnectionStrings:SifpDatabase` |
| `No migrations were applied` padahal tabel belum ada | Jalankan `dotnet build` sebelum `database update` |
| `Folder data hasil konversi tidak ditemukan` | Wajar bila `sifp_vue.client/src/data/generated` kosong — jalankan `npm run convert:excel`, atau abaikan dan import lewat API |
| Sheet wajib tidak ditemukan saat import | Workbook tidak memuat 14 sheet di `Helpers/SheetSchema.cs`; pesan errornya menyebut sheet mana yang kurang |
| `.xls` ditolak | ClosedXML hanya membaca format OpenXML; simpan ulang sebagai `.xlsx` |
| CORS error dari Vue | Hanya terjadi bila `VITE_API_BASE` diisi domain lain — daftarkan domain itu pada `Cors:AllowedOrigins` |
| Vue menampilkan "Tidak dapat memuat data" | Backend belum jalan atau port-nya bukan 5250; jalankan `dotnet run`, atau sesuaikan `VITE_API_PROXY` |
| Sidebar kosong / "Belum ada import" | Belum ada workbook yang berhasil diimport; jalankan seeder atau import lewat `/import` |
