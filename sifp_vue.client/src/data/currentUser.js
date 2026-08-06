// Identitas pengguna yang tampil di pojok kanan atas.
//
// SEMENTARA: aplikasi Vue belum punya halaman login sendiri, jadi untuk saat ini
// ditampilkan akun admin bawaan (lihat Seed:Admin* di appsettings server).
// Ketika login aplikasi Vue sudah dibuat, ganti nilai ini dengan data user dari
// sesi/endpoint (mis. /api/auth/me).
export const currentUser = {
  name: 'Administrator',
  role: 'Admin',
}
