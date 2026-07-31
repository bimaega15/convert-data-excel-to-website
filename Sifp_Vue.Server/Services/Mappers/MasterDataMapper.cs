using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Services.Mappers
{
    /// <summary>
    /// Pemetaan entity → DTO ditulis manual (tanpa AutoMapper) supaya bentuk JSON
    /// yang dikonsumsi Vue terlihat eksplisit dan tidak berubah diam-diam saat
    /// nama properti entity diubah.
    /// </summary>
    public static class MasterDataMapper
    {
        /// <summary>Excel menyimpan flag sebagai "Y"/"N"; bentuk itu dipertahankan di API.</summary>
        public static string ToYesNo(bool value) => value ? "Y" : "N";

        public static bool FromYesNo(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            (value.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) ||
             value.Trim().Equals("YES", StringComparison.OrdinalIgnoreCase) ||
             value.Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
             value.Trim() == "1");

        public static string? ToIsoDate(DateOnly? date) => date?.ToString("yyyy-MM-dd");

        public static ObservationDto ToDto(this Observation e) => new()
        {
            Id = e.ObsCode,
            Key = e.Id,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            Date = ToIsoDate(e.ObservationDate),
            Zona = e.Zona,
            Site = e.Site,
            Area = e.AreaEquipment,
            Activity = e.Activity,
            Company = e.Company,
            Observers = new[] { e.Observer1, e.Observer2, e.Observer3 }
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o!)
                .ToList(),
            Yes = e.YesCount,
            No = e.NoCount,
            Na = e.NaCount,
            Performance = e.PerformancePercent,
            Sequence = e.ObservationSequence,
            PsieEligible = ToYesNo(e.PsieEligible),
            Status = e.Status,
            Active = ToYesNo(e.IsActive)
        };

        public static void ApplyTo(this ObservationRequest request, Observation target)
        {
            target.ObsCode = request.ObsCode.Trim();
            target.ProtocolCode = request.ProtocolCode;
            target.ProtocolName = request.ProtocolName;
            target.ObservationDate = request.ObservationDate;
            target.Zona = request.Zona;
            target.Site = request.Site;
            target.AreaEquipment = request.AreaEquipment;
            target.Activity = request.Activity;
            target.Company = request.Company;
            target.Observer1 = request.Observer1;
            target.Observer2 = request.Observer2;
            target.Observer3 = request.Observer3;
            target.YesCount = request.YesCount;
            target.NoCount = request.NoCount;
            target.NaCount = request.NaCount;
            target.PerformancePercent = request.PerformancePercent;
            target.ObservationSequence = request.ObservationSequence;
            target.PsieEligible = request.PsieEligible;
            target.Status = request.Status;
            target.IsActive = request.IsActive;
        }

        public static ObservationRequest ToRequest(this Observation e) => new()
        {
            ObsCode = e.ObsCode,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            ObservationDate = e.ObservationDate,
            Zona = e.Zona,
            Site = e.Site,
            AreaEquipment = e.AreaEquipment,
            Activity = e.Activity,
            Company = e.Company,
            Observer1 = e.Observer1,
            Observer2 = e.Observer2,
            Observer3 = e.Observer3,
            YesCount = e.YesCount,
            NoCount = e.NoCount,
            NaCount = e.NaCount,
            PerformancePercent = e.PerformancePercent,
            ObservationSequence = e.ObservationSequence,
            PsieEligible = e.PsieEligible,
            Status = e.Status,
            IsActive = e.IsActive
        };

        public static SifQuestionDto ToDto(this SifQuestion e) => new()
        {
            Key = e.Id,
            ObsId = e.Observation?.ObsCode ?? string.Empty,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            QuestionRef = e.QuestionRef,
            CcvcId = e.CcvcId,
            Question = e.QuestionText,
            Answer = e.Answer,
            Comments = e.Comments,
            SifExposure = e.SifExposure,
            CriticalSafeguard = e.CriticalSafeguard,
            Date = ToIsoDate(e.ObservationDate),
            Zona = e.Zona,
            Site = e.Site,
            Activity = e.Activity,
            Company = e.Company
        };

        public static CcvcLibraryItemDto ToDto(this CcvcLibraryItem e) => new()
        {
            Key = e.Id,
            No = e.RowNo,
            ProtocolGroup = e.ProtocolGroup,
            PsecId = e.PsecId,
            PsecName = e.PsecName,
            ExposureType = e.ExposureType,
            CcvcId = e.CcvcId,
            QuestionCode = e.QuestionCode,
            QuestionSummary = e.QuestionSummary,
            VerificationPurpose = e.VerificationPurpose
        };

        public static ErrorTrapDto ToDto(this ErrorTrap e) => new()
        {
            Key = e.Id,
            ObsId = e.Observation?.ObsCode ?? string.Empty,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            Category = e.Category,
            ErrorTrap = e.TrapName,
            Comments = e.Comments
        };

        public static HpToolDto ToDto(this HpTool e) => new()
        {
            Key = e.Id,
            ObsId = e.Observation?.ObsCode ?? string.Empty,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            Tool = e.ToolName,
            Tujuan = e.Tujuan,
            KapanDigunakan = e.KapanDigunakan,
            CaraPakai = e.CaraPakai,
            EffectivenessNotes = e.EffectivenessNotes
        };

        public static DriftConditionDto ToDto(this DriftCondition e) => new()
        {
            Key = e.Id,
            ObsId = e.Observation?.ObsCode ?? string.Empty,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            Situation = e.Situation,
            Level1 = e.Level1,
            Code = e.Code,
            Level2 = e.Level2,
            Reason = e.Reason,
            Sequence = e.Sequence,
            Status = e.Status,
            Active = ToYesNo(e.IsActive)
        };

        public static LatentConditionDto ToDto(this LatentCondition e) => new()
        {
            Key = e.Id,
            ObsId = e.Observation?.ObsCode ?? string.Empty,
            ProtocolCode = e.ProtocolCode,
            ProtocolName = e.ProtocolName,
            Observation = e.ObservationText,
            Level1 = e.Level1,
            Code = e.Code,
            Level2 = e.Level2,
            Reason = e.Reason,
            Sequence = e.Sequence,
            Status = e.Status,
            Active = ToYesNo(e.IsActive)
        };

        public static InitiativeDto ToDto(this ImprovementInitiative e) => new()
        {
            Id = e.ImprovementCode,
            Key = e.Id,
            Initiative = e.Initiative,
            RelatedClsr = e.RelatedClsr,
            Owner = e.Owner,
            Status = e.Status,
            Progress = e.ProgressPercent,
            ExpectedImpact = e.ExpectedImpact,
            Notes = e.Notes
        };

        public static void ApplyTo(this InitiativeRequest request, ImprovementInitiative target)
        {
            target.ImprovementCode = request.ImprovementCode.Trim();
            target.Initiative = request.Initiative;
            target.RelatedClsr = request.RelatedClsr;
            target.Owner = request.Owner;
            target.Status = request.Status;
            target.ProgressPercent = request.ProgressPercent;
            target.ExpectedImpact = request.ExpectedImpact;
            target.Notes = request.Notes;
        }

        public static InitiativeRequest ToRequest(this ImprovementInitiative e) => new()
        {
            ImprovementCode = e.ImprovementCode,
            Initiative = e.Initiative,
            RelatedClsr = e.RelatedClsr,
            Owner = e.Owner,
            Status = e.Status,
            ProgressPercent = e.ProgressPercent,
            ExpectedImpact = e.ExpectedImpact,
            Notes = e.Notes
        };

        public static UserDto ToDto(this User e) => new()
        {
            Id = e.Id,
            Username = e.Username,
            Email = e.Email,
            FullName = e.FullName,
            Zona = e.Zona,
            IsActive = e.IsActive,
            LastLoginAt = e.LastLoginAt,
            Roles = e.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).OrderBy(n => n).ToList(),
            CanAccessAdmin = e.UserRoles.Any(r => r.Role != null && r.Role.CanAccessAdmin)
        };

        public static ImportBatchDto ToDto(this ImportBatch e) => new()
        {
            Id = e.Id,
            FileName = e.FileName,
            FileSizeBytes = e.FileSizeBytes,
            Status = e.Status.ToString(),
            SheetCount = e.SheetCount,
            TotalRows = e.TotalRows,
            EditCount = e.EditCount,
            ErrorMessage = e.ErrorMessage,
            CreatedAt = e.CreatedAt,
            CreatedBy = e.CreatedBy,
            CompletedAt = e.CompletedAt
        };
    }
}
