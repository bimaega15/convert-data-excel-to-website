using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Repositories
{
    public interface ICcvcLibraryRepository : IRepository<CcvcLibraryItem>
    {
        IQueryable<CcvcLibraryItem> Filter(CcvcLibraryQuery query);
        Task<CcvcLibraryItem?> GetByCcvcIdAsync(string ccvcId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetDistinctPsecIdsAsync(CancellationToken cancellationToken = default);
    }

    public class CcvcLibraryRepository : Repository<CcvcLibraryItem>, ICcvcLibraryRepository
    {
        public CcvcLibraryRepository(SifpDbContext context) : base(context)
        {
        }

        public IQueryable<CcvcLibraryItem> Filter(CcvcLibraryQuery query)
        {
            var search = query.Search?.Trim();

            return Query()
                .WhereIf(!string.IsNullOrEmpty(query.PsecId), x => x.PsecId == query.PsecId)
                .WhereIf(!string.IsNullOrEmpty(query.ProtocolGroup), x => x.ProtocolGroup == query.ProtocolGroup)
                .WhereIf(!string.IsNullOrEmpty(query.ExposureType), x => x.ExposureType == query.ExposureType)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    x.CcvcId.Contains(search!) ||
                    (x.PsecName != null && x.PsecName.Contains(search!)) ||
                    (x.QuestionSummary != null && x.QuestionSummary.Contains(search!)) ||
                    (x.VerificationPurpose != null && x.VerificationPurpose.Contains(search!)))
                .OrderBy(x => x.RowNo)
                .ThenBy(x => x.CcvcId);
        }

        public Task<CcvcLibraryItem?> GetByCcvcIdAsync(string ccvcId, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.CcvcId == ccvcId, cancellationToken);

        public async Task<IReadOnlyList<string>> GetDistinctPsecIdsAsync(CancellationToken cancellationToken = default)
            => await Query()
                .Where(x => x.PsecId != null)
                .Select(x => x.PsecId!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
    }

    public interface IInitiativeRepository : IRepository<ImprovementInitiative>
    {
        IQueryable<ImprovementInitiative> Filter(InitiativeQuery query);
        Task<ImprovementInitiative?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> CodeExistsAsync(string code, int? exceptId = null, CancellationToken cancellationToken = default);
    }

    public class InitiativeRepository : Repository<ImprovementInitiative>, IInitiativeRepository
    {
        public InitiativeRepository(SifpDbContext context) : base(context)
        {
        }

        public IQueryable<ImprovementInitiative> Filter(InitiativeQuery query)
        {
            var search = query.Search?.Trim();

            return Query()
                .WhereIf(!string.IsNullOrEmpty(query.Status), x => x.Status == query.Status)
                .WhereIf(!string.IsNullOrEmpty(query.Owner), x => x.Owner == query.Owner)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    x.ImprovementCode.Contains(search!) ||
                    (x.Initiative != null && x.Initiative.Contains(search!)) ||
                    (x.RelatedClsr != null && x.RelatedClsr.Contains(search!)) ||
                    (x.ExpectedImpact != null && x.ExpectedImpact.Contains(search!)))
                .OrderBy(x => x.ImprovementCode);
        }

        public Task<ImprovementInitiative?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.ImprovementCode == code, cancellationToken);

        public Task<bool> CodeExistsAsync(string code, int? exceptId = null, CancellationToken cancellationToken = default)
            => Query().AnyAsync(x => x.ImprovementCode == code && (exceptId == null || x.Id != exceptId), cancellationToken);
    }
}
