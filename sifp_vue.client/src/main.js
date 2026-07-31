import { createApp } from 'vue'
import App from './App.vue'
import bootstrap from './plugins/bootstrap'
import router from './router'
import { loadDashboard } from './data/dashboard'
import { loadSheetManifest } from './data/sheets'
import './assets/dashboard.css'

// Dashboard dan sidebar membaca datanya secara sinkron saat komponen dibuat,
// jadi keduanya diambil lebih dulu dan app baru di-mount setelah data siap.
// Ini juga membuat halaman tidak sempat "berkedip" dari kosong ke terisi.
async function start() {
  await Promise.all([loadDashboard(), loadSheetManifest()])
  createApp(App).use(bootstrap).use(router).mount('#app')
}

// Tanpa backend, aplikasi tidak punya data sama sekali. Lebih baik menjelaskan
// penyebabnya daripada menampilkan halaman kosong tanpa keterangan.
function showStartupError(error) {
  const target = document.querySelector('#app')
  if (!target) return

  const detail = error?.message ?? String(error)
  target.innerHTML = `
    <div class="boot-error">
      <div class="boot-error__card">
        <h1>Tidak dapat memuat data</h1>
        <p class="boot-error__detail"></p>
        <p class="boot-error__hint">
          Aplikasi ini mengambil datanya dari backend. Pastikan server sudah berjalan:
        </p>
        <pre>cd Sifp_Vue.Server
dotnet run</pre>
        <p class="boot-error__hint">
          Saat dev, permintaan <code>/api</code> diteruskan ke
          <code>http://localhost:5250</code> lewat proxy di <code>vite.config.js</code>.
          Ubah lewat <code>VITE_API_BASE</code> bila backend berjalan di alamat lain.
        </p>
        <button type="button" class="boot-error__retry">Coba lagi</button>
      </div>
    </div>`

  // textContent, bukan innerHTML: pesan error bisa memuat potongan respons server.
  target.querySelector('.boot-error__detail').textContent = detail
  target.querySelector('.boot-error__retry').addEventListener('click', () => window.location.reload())
}

start().catch((error) => {
  console.error('[SIFP] gagal memuat data awal:', error)
  showStartupError(error)
})
