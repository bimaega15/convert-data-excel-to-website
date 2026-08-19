using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Services.Contracts
{
    public class AuditLogQuery : QueryParameters
    {
        public string? Module { get; set; }
        public string? Username { get; set; }
    }

    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public int? StatusCode { get; set; }
    }

    public interface IAuditLogService
    {
        Task LogAsync(string username, string action, string module, string? details = null, string? ipAddress = null, int? statusCode = 200, CancellationToken cancellationToken = default);
        Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    }
}
