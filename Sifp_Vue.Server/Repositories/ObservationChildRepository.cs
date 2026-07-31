using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Repositories
{
    /// <summary>
    /// Kontrak repository untuk tabel yang selalu tergantung pada sebuah observasi.
    /// Filter dasar (per observasi, zona, protokol) sama untuk semuanya.
    /// </summary>
    public interface IObservationChildRepository<T> : IRepository<T> where T : class, IObservationChild
    {
        IQueryable<T> Filter(MasterDataQuery query);
        Task<IReadOnlyList<T>> GetByObservationIdAsync(int observationId, CancellationToken cancellationToken = default);
    }

    public class ObservationChildRepository<T> : Repository<T>, IObservationChildRepository<T>
        where T : class, IObservationChild
    {
        public ObservationChildRepository(SifpDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Selalu meng-Include observasi induk: DTO memerlukan ObsCode, dan tanpa
        /// Include setiap baris akan memicu query terpisah saat diproyeksikan.
        /// </summary>
        public override IQueryable<T> Query() => base.Query().Include(x => x.Observation);

        public virtual IQueryable<T> Filter(MasterDataQuery query)
        {
            return Query()
                .WhereIf(!string.IsNullOrEmpty(query.ObsCode), x => x.Observation!.ObsCode == query.ObsCode)
                .WhereIf(query.Zona.HasValue, x => x.Observation!.Zona == query.Zona)
                .WhereIf(!string.IsNullOrEmpty(query.ProtocolCode), x => x.ProtocolCode == query.ProtocolCode)
                .OrderBy(x => x.ObservationId)
                .ThenBy(x => x.Id);
        }

        public async Task<IReadOnlyList<T>> GetByObservationIdAsync(int observationId, CancellationToken cancellationToken = default)
            => await Query().Where(x => x.ObservationId == observationId).ToListAsync(cancellationToken);
    }

    // ---------- Repository spesifik: hanya menambah filter khas tabelnya ----------

    public interface ISifQuestionRepository : IObservationChildRepository<SifQuestion>
    {
        IQueryable<SifQuestion> Filter(SifQuestionQuery query);
    }

    public class SifQuestionRepository : ObservationChildRepository<SifQuestion>, ISifQuestionRepository
    {
        public SifQuestionRepository(SifpDbContext context) : base(context)
        {
        }

        public IQueryable<SifQuestion> Filter(SifQuestionQuery query)
        {
            var search = query.Search?.Trim();

            return base.Filter(query)
                .WhereIf(!string.IsNullOrEmpty(query.Answer), x => x.Answer == query.Answer)
                .WhereIf(!string.IsNullOrEmpty(query.CcvcId), x => x.CcvcId == query.CcvcId)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    (x.QuestionText != null && x.QuestionText.Contains(search!)) ||
                    (x.SifExposure != null && x.SifExposure.Contains(search!)) ||
                    (x.CriticalSafeguard != null && x.CriticalSafeguard.Contains(search!)) ||
                    (x.Comments != null && x.Comments.Contains(search!)));
        }
    }

    public interface IErrorTrapRepository : IObservationChildRepository<ErrorTrap>
    {
    }

    public class ErrorTrapRepository : ObservationChildRepository<ErrorTrap>, IErrorTrapRepository
    {
        public ErrorTrapRepository(SifpDbContext context) : base(context)
        {
        }

        public override IQueryable<ErrorTrap> Filter(MasterDataQuery query)
        {
            var search = query.Search?.Trim();

            return base.Filter(query)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    (x.TrapName != null && x.TrapName.Contains(search!)) ||
                    (x.Category != null && x.Category.Contains(search!)) ||
                    (x.Comments != null && x.Comments.Contains(search!)));
        }
    }

    public interface IHpToolRepository : IObservationChildRepository<HpTool>
    {
    }

    public class HpToolRepository : ObservationChildRepository<HpTool>, IHpToolRepository
    {
        public HpToolRepository(SifpDbContext context) : base(context)
        {
        }

        public override IQueryable<HpTool> Filter(MasterDataQuery query)
        {
            var search = query.Search?.Trim();

            return base.Filter(query)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    (x.ToolName != null && x.ToolName.Contains(search!)) ||
                    (x.Tujuan != null && x.Tujuan.Contains(search!)) ||
                    (x.EffectivenessNotes != null && x.EffectivenessNotes.Contains(search!)));
        }
    }

    public interface IDriftConditionRepository : IObservationChildRepository<DriftCondition>
    {
    }

    public class DriftConditionRepository : ObservationChildRepository<DriftCondition>, IDriftConditionRepository
    {
        public DriftConditionRepository(SifpDbContext context) : base(context)
        {
        }

        public override IQueryable<DriftCondition> Filter(MasterDataQuery query)
        {
            var search = query.Search?.Trim();

            return base.Filter(query)
                .WhereIf(!string.IsNullOrEmpty(query.Status), x => x.Status == query.Status)
                .WhereIf(query.IsActive.HasValue, x => x.IsActive == query.IsActive!.Value)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    (x.Situation != null && x.Situation.Contains(search!)) ||
                    (x.Level1 != null && x.Level1.Contains(search!)) ||
                    (x.Level2 != null && x.Level2.Contains(search!)) ||
                    (x.Reason != null && x.Reason.Contains(search!)));
        }
    }

    public interface ILatentConditionRepository : IObservationChildRepository<LatentCondition>
    {
    }

    public class LatentConditionRepository : ObservationChildRepository<LatentCondition>, ILatentConditionRepository
    {
        public LatentConditionRepository(SifpDbContext context) : base(context)
        {
        }

        public override IQueryable<LatentCondition> Filter(MasterDataQuery query)
        {
            var search = query.Search?.Trim();

            return base.Filter(query)
                .WhereIf(!string.IsNullOrEmpty(query.Status), x => x.Status == query.Status)
                .WhereIf(query.IsActive.HasValue, x => x.IsActive == query.IsActive!.Value)
                .WhereIf(!string.IsNullOrEmpty(search), x =>
                    (x.ObservationText != null && x.ObservationText.Contains(search!)) ||
                    (x.Level1 != null && x.Level1.Contains(search!)) ||
                    (x.Level2 != null && x.Level2.Contains(search!)) ||
                    (x.Reason != null && x.Reason.Contains(search!)));
        }
    }
}
