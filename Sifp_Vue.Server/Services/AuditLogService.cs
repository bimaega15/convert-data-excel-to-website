using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly SifpDbContext _db;

        public AuditLogService(SifpDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string username, string action, string module, string? details = null, string? ipAddress = null, int? statusCode = 200, CancellationToken cancellationToken = default)
        {
            try
            {
                var log = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    Username = string.IsNullOrWhiteSpace(username) ? "ANONYMOUS" : username,
                    Action = action,
                    Module = module,
                    Details = details,
                    IpAddress = ipAddress,
                    StatusCode = statusCode
                };

                _db.AuditLogs.Add(log);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Pengabaian eror internal agar logging tidak merusak alur transaksi utama
            }
        }

        public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
        {
            var q = _db.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Module))
            {
                q = q.Where(x => x.Module.ToLower() == query.Module.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Username))
            {
                q = q.Where(x => x.Username.ToLower().Contains(query.Username.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                q = q.Where(x => x.Action.ToLower().Contains(term) || (x.Details != null && x.Details.ToLower().Contains(term)));
            }

            q = q.OrderByDescending(x => x.Timestamp);

            return await q.ToPagedResultAsync(query, x => new AuditLogDto
            {
                Id = x.Id,
                Timestamp = x.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Username = x.Username,
                Action = x.Action,
                Module = x.Module,
                Details = x.Details,
                IpAddress = x.IpAddress,
                StatusCode = x.StatusCode
            }, cancellationToken);
        }
    }
}
