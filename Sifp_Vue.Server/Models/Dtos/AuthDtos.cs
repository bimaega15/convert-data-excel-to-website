using System.ComponentModel.DataAnnotations;

namespace Sifp_Vue.Server.Models.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Login API Vue saja; cookie admin selalu pakai masa berlaku tetap.</summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>Hasil login berhasil untuk klien Vue: token bearer + data user.</summary>
    public class LoginResultDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public UserDto User { get; set; } = null!;
    }

    /// <summary>
    /// Dibalas setelah username/password (atau login Microsoft) benar, tapi sebelum
    /// kode MFA diverifikasi — belum ada token bearer di sini. <see cref="SetupRequired"/>
    /// true berarti akun ini belum pernah mengaktifkan MFA, jadi klien perlu
    /// menampilkan QR code untuk di-scan sebelum meminta kode pertama.
    /// </summary>
    public class LoginChallengeDto
    {
        public string ChallengeToken { get; set; } = string.Empty;
        public bool SetupRequired { get; set; }

        /// <summary>Data URI PNG, hanya diisi saat <see cref="SetupRequired"/> true.</summary>
        public string? QrCodeDataUri { get; set; }

        /// <summary>Secret dalam format yang gampang diketik manual, fallback bila QR tidak bisa di-scan.</summary>
        public string? ManualEntryKey { get; set; }
    }

    public class MfaVerifyRequest
    {
        [Required(ErrorMessage = "Sesi verifikasi tidak ditemukan.")]
        public string ChallengeToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kode MFA wajib diisi.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Kode MFA harus 6 digit angka.")]
        public string Code { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int? Zona { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool CanAccessAdmin { get; set; }
    }

    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Password wajib diisi.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password minimal 8 karakter.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Range(1, 99)]
        public int? Zona { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> RoleIds { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? FullName { get; set; }

        /// <summary>Dikosongkan bila password tidak diubah.</summary>
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password minimal 8 karakter.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Range(1, 99)]
        public int? Zona { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> RoleIds { get; set; } = new();
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool CanAccessAdmin { get; set; }
        public int UserCount { get; set; }
    }
}
