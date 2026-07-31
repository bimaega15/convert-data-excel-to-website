namespace Sifp_Vue.Server.Models.Dtos
{
    /// <summary>Filter umum untuk seluruh tabel master data yang terikat observasi.</summary>
    public class MasterDataQuery : QueryParameters
    {
        /// <summary>Batasi ke satu observasi, mis. "OBS-001".</summary>
        public string? ObsCode { get; set; }

        public int? Zona { get; set; }
        public string? ProtocolCode { get; set; }
        public string? Status { get; set; }

        /// <summary>Bila diisi, hanya baris aktif (atau non-aktif) yang dikembalikan.</summary>
        public bool? IsActive { get; set; }
    }

    public class ObservationQuery : MasterDataQuery
    {
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public string? Site { get; set; }
        public string? Company { get; set; }
    }

    public class SifQuestionQuery : MasterDataQuery
    {
        /// <summary>YES / NO / NA.</summary>
        public string? Answer { get; set; }

        public string? CcvcId { get; set; }
    }

    public class CcvcLibraryQuery : QueryParameters
    {
        public string? PsecId { get; set; }
        public string? ProtocolGroup { get; set; }
        public string? ExposureType { get; set; }
    }

    public class InitiativeQuery : QueryParameters
    {
        public string? Status { get; set; }
        public string? Owner { get; set; }
    }
}
