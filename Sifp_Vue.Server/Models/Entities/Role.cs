namespace Sifp_Vue.Server.Models.Entities
{
    public static class RoleNames
    {
        public const string Administrator = "Administrator";
        public const string Verifier = "Verifier";
        public const string Viewer = "Viewer";
    }

    public class Role : AuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>True untuk role yang boleh membuka area /admin (Razor MVC).</summary>
        public bool CanAccessAdmin { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
