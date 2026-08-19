namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Akun aplikasi. Dipakai login cookie area /admin.</summary>
    public class User : AuditableEntity
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }

        /// <summary>Hash PBKDF2 berformat <c>iterations.salt.hash</c> (lihat PasswordHasher).</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Zona kerja user, dipakai untuk memfilter data observasi.</summary>
        public int? Zona { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        /// <summary>True setelah user berhasil memverifikasi kode pertama saat setup MFA.</summary>
        public bool MfaEnabled { get; set; }

        /// <summary>Secret TOTP (Base32), diisi begitu setup MFA dikonfirmasi. Null sebelum itu.</summary>
        public string? MfaSecret { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
