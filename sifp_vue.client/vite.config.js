import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  // Backend ASP.NET Core (Sifp_Vue.Server). Sesuaikan lewat VITE_API_PROXY bila
  // server dijalankan di port lain.
  const target = env.VITE_API_PROXY || 'http://localhost:5250'

  return {
    plugins: [vue()],
    server: {
      port: 5173,
      // Permintaan /api diteruskan ke backend supaya klien dan API berada pada
      // origin yang sama saat dev — persis seperti di produksi (Vue disajikan
      // dari wwwroot server), jadi tidak ada perbedaan perilaku antar mode.
      // Catatan: alur "Sign in with Microsoft" (OIDC) sengaja menuju backend
      // :5250 langsung, tidak lewat proxy ini.
      proxy: {
        '/api': { target, changeOrigin: true },
      },
    },
  }
})
