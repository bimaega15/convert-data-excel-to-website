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
