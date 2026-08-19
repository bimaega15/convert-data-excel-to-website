# Sign in with Microsoft — Panduan Konfigurasi (Microsoft Entra ID / OIDC)

Dokumen ini menjelaskan cara mengaktifkan tombol **"Sign in with Microsoft"** pada
halaman login SIFP Assurance. Kode aplikasi sudah siap; yang belum hanya kredensial
**App registration** di tenant Microsoft 365 Pertamina. Selama kredensial belum diisi,
tombol tetap tampil tetapi menampilkan pesan *"Login Microsoft belum dikonfigurasi"*.

## Cara kerja singkat

```
Tombol  ──►  /api/auth/microsoft/login  ──►  Halaman login Microsoft
                                                     │  (user login @pertamina.com)
   Vue /auth/mfa  ◄── #challenge=…  ◄── /api/auth/microsoft/callback  ◄──┘
        (domain @pertamina.com + user terdaftar dicek, lalu tantangan MFA diterbitkan —
         JWT aplikasi baru keluar setelah kode 6 digit authenticator diverifikasi)
```

- **Auto-provisioning aktif** (`Auth:AutoProvision = true`): setiap akun `@pertamina.com`
  yang berhasil login Microsoft otomatis dibuatkan akun aplikasi dengan role default
  **Viewer** (read-only), lalu diminta setup MFA (akun baru) atau kode MFA (akun lama)
  sebelum masuk ke dashboard — tanpa perlu didaftarkan admin terlebih dulu.
- Domain wajib `@pertamina.com` (lihat `Auth:AllowedEmailDomains`).
- Tenant di-set **single-tenant**, jadi hanya akun organisasi Pertamina yang bisa login.

---

## Langkah 1 — Daftarkan aplikasi di Azure Portal

1. Buka **Azure Portal → Microsoft Entra ID → App registrations → New registration**.
2. **Name**: `SIFP Assurance Dashboard`.
3. **Supported account types**: pilih **Accounts in this organizational directory only
   (Single tenant)**.
4. **Redirect URI**: platform **Web**, isi sesuai lingkungan:
   - Development: `http://localhost:5250/signin-oidc`
   - Produksi: `https://<domain-produksi>/signin-oidc`

   (Boleh menambah lebih dari satu Redirect URI di menu **Authentication** setelah dibuat.)
5. Klik **Register**.
6. Di halaman **Overview**, catat:
   - **Application (client) ID** → `AzureAd:ClientId`
   - **Directory (tenant) ID** → `AzureAd:TenantId`

## Langkah 2 — Buat Client Secret

1. Menu **Certificates & secrets → Client secrets → New client secret**.
2. Beri deskripsi + masa berlaku, klik **Add**.
3. **Salin nilai (Value) sekarang juga** (hanya tampil sekali) → `AzureAd:ClientSecret`.

## Langkah 3 — Izin API (biasanya sudah default)

Menu **API permissions** harus memuat Microsoft Graph **Delegated**: `openid`, `profile`,
`email`, `User.Read`. Ini default saat registrasi. Klik **Grant admin consent** bila
diminta kebijakan tenant.

---

## Langkah 4 — Isi konfigurasi aplikasi

Kunci konfigurasi (section `AzureAd`):

| Kunci | Nilai |
|-------|-------|
| `AzureAd:Instance` | `https://login.microsoftonline.com/` (biarkan default) |
| `AzureAd:TenantId` | Directory (tenant) ID |
| `AzureAd:ClientId` | Application (client) ID |
| `AzureAd:ClientSecret` | Client secret (Value) |
| `AzureAd:CallbackPath` | `/signin-oidc` (harus cocok dengan Redirect URI) |

**Jangan** menaruh `ClientSecret` di `appsettings.json` yang ikut ke git. Gunakan
**user-secrets** (dev) atau **environment variable** (produksi).

### Development (user-secrets)

```bash
cd convert-data-excel-to-website/Sifp_Vue.Server
dotnet user-secrets set "AzureAd:TenantId"     "<TENANT_ID>"
dotnet user-secrets set "AzureAd:ClientId"     "<CLIENT_ID>"
dotnet user-secrets set "AzureAd:ClientSecret" "<CLIENT_SECRET>"
```

> `Auth:ClientBaseUrl` di dev sudah di-set ke `http://localhost:5173`
> (di `appsettings.Development.json`) sehingga setelah login user dikembalikan ke Vite.

### Produksi (environment variable)

```
ConnectionStrings__SifpDatabase=...
AzureAd__TenantId=<TENANT_ID>
AzureAd__ClientId=<CLIENT_ID>
AzureAd__ClientSecret=<CLIENT_SECRET>
Auth__ClientBaseUrl=            # kosongkan: Vue & API satu origin di produksi
```

## Langkah 5 — Kebijakan user (auto-provisioning)

Karena `Auth:AutoProvision = true`, kamu **tidak perlu** mendaftarkan user satu per satu.
Siapa pun dengan akun `@pertamina.com` yang lolos login Microsoft otomatis dibuatkan akun
role **Viewer** dan langsung masuk dashboard.

- **Menaikkan hak akses** (mis. jadi Verifier/Administrator): buka **/admin/users**, cari
  user tersebut (muncul setelah login pertamanya), ubah role-nya.
- **Menonaktifkan seseorang**: set akun-nya non-aktif di /admin/users — login berikutnya
  akan ditolak.

### Bila ingin kembali ke mode "hanya yang terdaftar"

Set `Auth:AutoProvision = false` (di `appsettings.json` atau environment variable
`Auth__AutoProvision=false`). Saat itu hanya email yang **sudah terdaftar & aktif** di
tabel Users yang diterima; email harus **sama persis** dengan UPN Microsoft
(`preferred_username`). Role default auto-provision diatur lewat `Auth:AutoProvisionRole`.

---

## Redirect URI per lingkungan (ringkasan)

| Lingkungan | Redirect URI di Azure | `Auth:ClientBaseUrl` |
|------------|----------------------|----------------------|
| Development | `http://localhost:5250/signin-oidc` | `http://localhost:5173` |
| Produksi | `https://<domain>/signin-oidc` | *(kosong)* |

Tombol SSO di dev sengaja menuju langsung ke backend `http://localhost:5250` (bukan lewat
proxy Vite) agar seluruh handshake OIDC berada pada satu origin.

## Troubleshooting

- **AADSTS50011 (redirect URI mismatch)**: Redirect URI di Azure tidak sama persis dengan
  `{host}{AzureAd:CallbackPath}`. Samakan skema/host/port/path.
- **"Correlation failed"**: cookie handshake tidak terkirim. Aplikasi sudah memakai
  `SameSite=Lax` + `response_mode=query` agar bekerja di http (dev) maupun https (prod).
  Pastikan mengakses lewat host yang sama dengan Redirect URI.
- **"Akun Microsoft ini belum terdaftar"**: email user belum ada di tabel Users, atau
  berbeda dari UPN Microsoft. Tambahkan/perbaiki lewat /admin/users.
- **Produksi wajib HTTPS**: token & cookie hanya boleh lewat koneksi aman.
