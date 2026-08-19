using System;

namespace Sifp_Vue.Server.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Username { get; set; } = "SYSTEM";
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = "SYSTEM";
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public int? StatusCode { get; set; }
    }
}
