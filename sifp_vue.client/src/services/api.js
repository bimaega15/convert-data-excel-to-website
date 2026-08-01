// Klien HTTP tunggal untuk seluruh pemanggilan backend.
//
// Semua endpoint /api membalas dengan amplop yang sama:
//   { status: "SUCCESS" | "ERROR", message, data, errors }
// Modul ini yang membuka amplop itu, sehingga pemanggil cukup menerima `data`
// dan tidak perlu memeriksa `status` di setiap tempat.

// Kosong = origin yang sama (mode produksi: Vue disajikan dari wwwroot server,
// dan saat dev vite.config.js mem-proxy /api ke backend).
export const API_BASE = import.meta.env.VITE_API_BASE ?? ''

export class ApiError extends Error {
  constructor(message, { status = 0, errors = null, url = '' } = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    /** Detail validasi per field, mis. { ObsCode: ["Obs ID wajib diisi."] } */
    this.errors = errors
    this.url = url
  }
}

function buildUrl(path, params) {
  const url = `${API_BASE}${path}`
  if (!params) return url

  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    // Parameter kosong tidak dikirim supaya URL tetap bersih dan filter
    // "semua" di UI tidak terkirim sebagai string kosong.
    if (value === null || value === undefined || value === '') continue
    query.append(key, String(value))
  }

  const qs = query.toString()
  return qs ? `${url}?${qs}` : url
}

/**
 * Memanggil backend lalu mengembalikan isi `data` dari amplop respons.
 * Melempar {@link ApiError} untuk semua kegagalan, termasuk gangguan jaringan,
 * supaya pemanggil hanya perlu menangani satu jenis error.
 */
export async function request(path, { method = 'GET', params, body, signal } = {}) {
  const url = buildUrl(path, params)

  const headers = {}
  let payload
  if (body instanceof FormData) {
    // Content-Type sengaja tidak diisi: browser harus menentukannya sendiri
    // agar boundary multipart ikut tertulis.
    payload = body
  } else if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
    payload = JSON.stringify(body)
  }

  let response
  try {
    response = await fetch(url, { method, headers, body: payload, signal })
  } catch (err) {
    if (err.name === 'AbortError') throw err
    throw new ApiError(`Tidak dapat menghubungi server (${url}). ${err.message}`, { url })
  }

  const text = await response.text()
  let envelope = null
  if (text) {
    try {
      envelope = JSON.parse(text)
    } catch {
      envelope = null
    }
  }

  if (!response.ok) {
    throw new ApiError(
      envelope?.message ?? `Server membalas HTTP ${response.status}.`,
      { status: response.status, errors: envelope?.errors ?? null, url }
    )
  }

  if (envelope && envelope.status === 'ERROR') {
    throw new ApiError(envelope.message ?? 'Permintaan ditolak server.', {
      status: response.status,
      errors: envelope.errors ?? null,
      url,
    })
  }

  // Endpoint yang tidak memakai amplop (tidak ada saat ini) tetap dikembalikan apa adanya.
  return envelope && 'data' in envelope ? envelope.data : envelope
}

export const api = {
  get: (path, options) => request(path, { ...options, method: 'GET' }),
  post: (path, body, options) => request(path, { ...options, method: 'POST', body }),
  put: (path, body, options) => request(path, { ...options, method: 'PUT', body }),
  delete: (path, options) => request(path, { ...options, method: 'DELETE' }),
}

/**
 * Mengambil seluruh baris dari endpoint berhalaman.
 * Halaman master data menampilkan tabel penuh dengan sort & filter di sisi klien,
 * jadi datanya diambil sekaligus, bukan per halaman.
 */
export async function fetchAllPages(path, { pageSize = 200, params, signal } = {}) {
  const rows = []
  let page = 1
  let totalPages = 1

  do {
    const result = await api.get(path, { params: { ...params, page, pageSize }, signal })
    rows.push(...(result?.items ?? []))
    totalPages = result?.totalPages ?? 1
    page += 1
  } while (page <= totalPages)

  return rows
}
