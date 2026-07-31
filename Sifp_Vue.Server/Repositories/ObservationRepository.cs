using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Repositories
{
    public interface IObservationRepository : IRepository<Observation>
    {
        IQueryable<Observation> Filter(ObservationQuery query);
        Task<Observation?> GetByObsCodeAsync(string obsCode, CancellationToken cancellationToken = default);
        Task<Observation?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ObsCodeExistsAsync(string obsCode, int? exceptId = null, CancellationToken cancellationToken = default);

        /// <summary>Peta ObsCode → Id, dipakai import untuk menautkan baris anak tanpa query per baris.</summary>
        Task<Dictionary<string, int>> GetIdMapAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetDistinctProtocolsAsync(CancellationToken cancellationToken = default);
    }

    public class ObservationRepository : Repository<Observation>, IObservationRepository
    {
        public ObservationRepository(SifpDbContext context) : base(context)
        {
        }

        public IQueryable<Observation> Filter(ObservationQuery query)
        {
            var search = query.Search?.Trim();

            var result = Query()
                .WhereIf(!string.IsNullOrEmpty(query.ObsCode), x => x.ObsCode == query.ObsCode)
                .WhereIf(query.Zona.HasValue, x => x.Zona == query.Zona)
                .WhereIf(!string.IsNullOrEmpty(query.ProtocolCode), x => x.ProtocolCode == query.ProtocolCode)
                .WhereIf(!string.IsNullOrEmpty(query.Status), x => x.Status == query.Status)
                .WhereIf(query.IsActive.HasValue, x => x.IsActive == query.IsActive!.Value)
                .WhereIf(query.DateFrom.HasValue, x => x.ObservationDate >= query.DateFrom)
                .WhereIf(query.DateTo.HasValue, x => x.ObservationDate <= query.DateTo)
                .WhereIf(!string.IsNullOrEmpty(query.Site), x => x.Site == query.Site)
                .WhereIf(!string.IsNullOrEmpty(query.Company), x => x.Company == query.Company)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    x.ObsCode.Contains(search!) ||
                    (x.ProtocolName != null && x.ProtocolName.Contains(search!)) ||
                    (x.Site != null && x.Site.Contains(search!)) ||
                    (x.Activity != null && x.Activity.Contains(search!)) ||
                    (x.Company != null && x.Company.Contains(search!)));

            return ApplySort(result, query);
        }

        // Sort ditulis eksplisit (bukan refleksi atas SortBy) supaya nama kolom dari
        // query string tidak pernah masuk ke SQL.
        private static IQueryable<Observation> ApplySort(IQueryable<Observation> query, ObservationQuery parameters)
        {
            var desc = parameters.SortDescending;
            return parameters.SortBy?.ToLowerInvariant() switch
            {
                "date" => desc ? query.OrderByDescending(x => x.ObservationDate) : query.OrderBy(x => x.ObservationDate),
                "zona" => desc ? query.OrderByDescending(x => x.Zona) : query.OrderBy(x => x.Zona),
                "performance" => desc ? query.OrderByDescending(x => x.PerformancePercent) : query.OrderBy(x => x.PerformancePercent),
                "site" => desc ? query.OrderByDescending(x => x.Site) : query.OrderBy(x => x.Site),
                "protocol" => desc ? query.OrderByDescending(x => x.ProtocolCode) : query.OrderBy(x => x.ProtocolCode),
                _ => desc ? query.OrderByDescending(x => x.ObsCode) : query.OrderBy(x => x.ObsCode)
            };
        }

        public Task<Observation?> GetByObsCodeAsync(string obsCode, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.ObsCode == obsCode, cancellationToken);

        public Task<Observation?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
            => Query()
                .Include(x => x.SifQuestions)
                .Include(x => x.ErrorTraps)
                .Include(x => x.HpTools)
                .Include(x => x.DriftConditions)
                .Include(x => x.LatentConditions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public Task<bool> ObsCodeExistsAsync(string obsCode, int? exceptId = null, CancellationToken cancellationToken = default)
            => Query().AnyAsync(x => x.ObsCode == obsCode && (exceptId == null || x.Id != exceptId), cancellationToken);

        public async Task<Dictionary<string, int>> GetIdMapAsync(CancellationToken cancellationToken = default)
            => await Query().ToDictionaryAsync(x => x.ObsCode, x => x.Id, cancellationToken);

        public async Task<IReadOnlyList<string>> GetDistinctProtocolsAsync(CancellationToken cancellationToken = default)
            => await Query()
                .Where(x => x.ProtocolCode != null)
                .Select(x => x.ProtocolCode!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
    }
}
