namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>Tabel penghubung many-to-many User ↔ Role.</summary>
    public class UserRole
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
    }
}
