// Data tantangan MFA sementara antara halaman login/callback SSO dan halaman
// MFA. Disimpan di sessionStorage (bukan localStorage seperti sesi login)
// karena umurnya pendek (challengeToken kedaluwarsa 5 menit) dan tidak perlu
// bertahan lintas tab atau setelah browser ditutup.

const STORAGE_KEY = 'sifp.mfa.pending'

export function setPendingMfaChallenge(challenge) {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(challenge))
}

export function getPendingMfaChallenge() {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) return null

  try {
    return JSON.parse(raw)
  } catch {
    return null
  }
}

export function clearPendingMfaChallenge() {
  sessionStorage.removeItem(STORAGE_KEY)
}
