namespace Sifp_Vue.Server.Models.Dtos
{
    /// <summary>
    /// Bentuknya sengaja dibuat sama persis dengan
    /// <c>sifp_vue.client/src/data/generated/dashboard.json</c>, sehingga
    /// <c>src/data/dashboard.js</c> cukup mengganti sumber import-nya ke endpoint
    /// <c>GET /api/dashboard</c> tanpa menyentuh komponen dashboard.
    /// </summary>
    public class DashboardDto
    {
        public DashboardMetaDto Meta { get; set; } = new();
        public List<KpiCardDto> Kpis { get; set; } = new();
        public ConformanceDto Conformance { get; set; } = new();
        public List<QuickFactDto> QuickFacts { get; set; } = new();
        public HealthMapDto HealthMap { get; set; } = new();
        public List<TopPanelDto> TopPanels { get; set; } = new();
        public TrendDto Trend { get; set; } = new();
        public ZonaScoresDto ZonaScores { get; set; } = new();
        public List<DashboardInitiativeDto> Initiatives { get; set; } = new();
        public List<SummaryCardDto> SummaryCards { get; set; } = new();
        public List<string> SummaryNotes { get; set; } = new();
        public string FooterNote { get; set; } = string.Empty;
    }

    public class DashboardMetaDto
    {
        public string Title { get; set; } = "REGIONAL 4 SIFP ASSURANCE DASHBOARD";
        public string Subtitle { get; set; } = string.Empty;
        public bool Draft { get; set; }
        public string? SourceFile { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class KpiCardDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Title { get; set; }
        public decimal Value { get; set; }

        /// <summary>True bila metrik belum punya data (mis. PSIE berstatus Pending).</summary>
        public bool Pending { get; set; }

        public string? Desc { get; set; }

        /// <summary>Varian warna kartu di Vue: green / blue / purple.</summary>
        public string Variant { get; set; } = "green";

        public string? Target { get; set; }
    }

    public class ConformanceDto
    {
        public decimal Value { get; set; }
        public string? Target { get; set; }
        public List<ConformanceBandDto> Bands { get; set; } = new();
    }

    public class ConformanceBandDto
    {
        public string Status { get; set; } = string.Empty;
        public int From { get; set; }
        public int To { get; set; }
    }

    public class QuickFactDto
    {
        public string Icon { get; set; } = "clipboard";
        public string Label { get; set; } = string.Empty;
        public string? Value { get; set; }
    }

    public class HealthMapDto
    {
        public List<string> Zones { get; set; } = new();
        public List<HealthMapRowDto> Rows { get; set; } = new();
    }

    public class HealthMapRowDto
    {
        public string? Name { get; set; }
        public List<HealthMapCellDto> Cells { get; set; } = new();
        public decimal? Regional { get; set; }
        public string RegionalStatus { get; set; } = "nodata";
    }

    public class HealthMapCellDto
    {
        /// <summary>effective / degraded / failed / nodata.</summary>
        public string Status { get; set; } = "nodata";

        public decimal? Score { get; set; }

        /// <summary>Angka yang dicetak di dalam sel — hanya diisi untuk status "failed".</summary>
        public decimal? Value { get; set; }
    }

    public class TopPanelDto
    {
        public int No { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Variant { get; set; }
        public string? Dash { get; set; }
        public List<TopPanelItemDto> Items { get; set; } = new();
        public TopPanelFooterDto Footer { get; set; } = new();
    }

    public class TopPanelItemDto
    {
        public string? Label { get; set; }

        /// <summary>Teks siap tampil, mis. "12 (52%)".</summary>
        public string? Display { get; set; }

        /// <summary>Rasio 0-1 terhadap item terbesar, dipakai sebagai lebar bar.</summary>
        public decimal Weight { get; set; }
    }

    public class TopPanelFooterDto
    {
        public string? Icon { get; set; }
        public string? Label { get; set; }
        public int Value { get; set; }
    }

    public class TrendDto
    {
        public decimal? Target { get; set; }
        public string? TargetLabel { get; set; }
        public List<TrendPointDto> Points { get; set; } = new();
        public List<TrendPointDto> Projection { get; set; } = new();
    }

    public class TrendPointDto
    {
        public string? Month { get; set; }
        public decimal Value { get; set; }
    }

    public class ZonaScoresDto
    {
        public decimal? Target { get; set; }
        public string? TargetLabel { get; set; }
        public List<ZonaBarDto> Bars { get; set; } = new();
    }

    public class ZonaBarDto
    {
        public string? Zone { get; set; }
        public int Obs { get; set; }
        public decimal Value { get; set; }
    }

    public class DashboardInitiativeDto
    {
        public string? Name { get; set; }
        public string? Owner { get; set; }
        public string? Status { get; set; }
        public int Progress { get; set; }
    }

    public class SummaryCardDto
    {
        public string? Icon { get; set; }
        public string? Tone { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
    }

    /// <summary>Ringkasan angka untuk kartu di halaman /admin (Razor), bukan untuk Vue.</summary>
    public class AdminDashboardSummaryDto
    {
        public int TotalObservations { get; set; }
        public int ActiveObservations { get; set; }
        public int TotalSifQuestions { get; set; }
        public int TotalFindings { get; set; }
        public int TotalInitiatives { get; set; }
        public int OpenInitiatives { get; set; }
        public decimal? ConformanceScore { get; set; }
        public int ZonesCovered { get; set; }
        public DateTime? LastImportAt { get; set; }
        public string? LastImportFile { get; set; }
        public List<ZonaBarDto> ZonaBreakdown { get; set; } = new();
    }
}
