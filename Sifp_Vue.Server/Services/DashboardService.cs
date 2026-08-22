using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Services
{
    /// <summary>
    /// Menyusun ulang isi <c>dashboard.json</c> dari tabel SQL Server.
    /// Aturan turunannya (pemetaan status, pemilihan varian warna, teks footer)
    /// disalin dari <c>sifp_vue.client/scripts/convert-excel.mjs</c> supaya hasil
    /// dashboard identik baik data berasal dari JSON statis maupun dari API.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly SifpDbContext _context;

        /// <summary>Pemetaan status di Excel → kelas status yang dipakai komponen Vue.</summary>
        private static readonly Dictionary<string, string> StatusKey = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Effective"] = "effective",
            ["Degraded"] = "degraded",
            ["Failed / High Concern"] = "failed",
            ["No Data"] = "nodata"
        };

        // Konfigurasi tampilan empat panel Top 5. Urutan menentukan urutan tampil.
        private static readonly (string Category, int No, string Title, string Subtitle, string Variant, string? Dash, string FooterIcon, string FooterLabel, bool WithPercent)[] TopPanelConfig =
        {
            ("Top SIF Exposure", 2, "TOP 5 SIF EXPOSURES", "(by Frequency)", "green", "green", "clipboard", "Total Observations", true),
            ("Top Critical Safeguard Gap", 3, "TOP 5 CRITICAL SAFEGUARD GAPS", "(by Frequency)", "red", "red", "shield", "Total Findings", false),
            ("Top Recurring Drift", 4, "TOP 5 RECURRING DRIFT", "(Observed)", "blue", "amber", "refresh", "Total Occurrences", false),
            ("Top Systemic Issue", 5, "TOP 5 SYSTEMIC ISSUES", "(Identified)", "purple", null, "gear", "Total Findings", false)
        };

        public DashboardService(SifpDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var measures = await _context.ExecutiveMeasures.AsNoTracking()
                .ToDictionaryAsync(x => x.MetricCode, x => x, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var quickFacts = await _context.QuickFacts.AsNoTracking()
                .OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);

            var healthRows = await _context.ClsrHealthMapRows.AsNoTracking()
                .OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);

            var topItems = await _context.TopFiveItems.AsNoTracking()
                .OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);

            var trendPoints = await _context.TrendPoints.AsNoTracking()
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.PeriodMonth).ToListAsync(cancellationToken);

            var zonaScores = await _context.ZonaScores.AsNoTracking()
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Zona).ToListAsync(cancellationToken);

            var initiatives = await _context.ImprovementInitiatives.AsNoTracking()
                .OrderBy(x => x.ImprovementCode).ToListAsync(cancellationToken);

            var texts = await _context.DashboardTexts.AsNoTracking()
                .ToDictionaryAsync(x => x.Section, x => x.Text ?? string.Empty, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var lastBatch = await _context.ImportBatches.AsNoTracking()
                .Where(x => x.Status == ImportStatus.Completed)
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            // Ambil bulan terbaru dari observation_date untuk dipakai di subtitle dashboard.
            // Ini menggantikan teks hard-coded "July 2026" — setiap kali ada data baru
            // yang diimport, nama bulan otomatis ikut berubah tanpa perlu edit manual.
            var maxObsDate = await _context.Observations.AsNoTracking()
                .Where(x => x.ObservationDate.HasValue)
                .MaxAsync(x => (DateOnly?)x.ObservationDate, cancellationToken);

            var maxMonthLabel = maxObsDate.HasValue
                ? maxObsDate.Value.ToDateTime(TimeOnly.MinValue)
                    .ToString("MMMM yyyy", CultureInfo.InvariantCulture)
                : "–";

            // Dikelompokkan di memori (bukan GroupBy di SQL) karena DateOnly belum
            // punya translator LINQ-to-SQL Server yang lengkap untuk Year/Month di EF Core 8.
            var obsMonths = await _context.Observations.AsNoTracking()
                .Where(x => x.ObservationDate.HasValue)
                .Select(x => x.ObservationDate!.Value)
                .ToListAsync(cancellationToken);

            var observationsByMonth = obsMonths
                .GroupBy(d => new DateOnly(d.Year, d.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new MonthlyObservationCountDto
                {
                    Month = g.Key.ToDateTime(TimeOnly.MinValue).ToString("MMM", CultureInfo.InvariantCulture),
                    Count = g.Count()
                })
                .ToList();

            var conformanceTarget = measures.GetValueOrDefault("CONF")?.TargetPercent ?? 80m;

            return new DashboardDto
            {
                Meta = new DashboardMetaDto
                {
                    Title = "REGIONAL 4 SIFP ASSURANCE DASHBOARD",
                    Subtitle = $"Executive Dashboard \u2013 Full Database ({maxMonthLabel})",
                    Draft = true,
                    SourceFile = lastBatch?.FileName,
                    GeneratedAt = lastBatch?.CompletedAt ?? lastBatch?.CreatedAt ?? DateTime.UtcNow
                },
                Kpis = BuildKpis(measures),
                Conformance = BuildConformance(measures.GetValueOrDefault("CONF")),
                QuickFacts = quickFacts.Select(x => new QuickFactDto
                {
                    Icon = x.Icon ?? "clipboard",
                    Label = x.FactName,
                    Value = x.FactValue
                }).ToList(),
                HealthMap = BuildHealthMap(healthRows),
                TopPanels = BuildTopPanels(topItems),
                Trend = BuildTrend(trendPoints, conformanceTarget),
                ZonaScores = BuildZonaScores(zonaScores, conformanceTarget),
                Initiatives = initiatives.Select(x => new DashboardInitiativeDto
                {
                    Name = x.Initiative,
                    Owner = x.Owner,
                    Status = x.Status,
                    Progress = x.ProgressPercent
                }).ToList(),
                SummaryCards = BuildSummaryCards(texts, topItems),
                SummaryNotes = BuildSummaryNotes(texts, quickFacts),
                FooterNote = "Dashboard ini menggunakan data observasi V&V (Full Database). " +
                             "Nilai indikator diperbarui otomatis dari hasil konversi Excel setiap kali " +
                             "data observasi bertambah dan tervalidasi.",
                ObservationsByMonth = observationsByMonth
            };
        }

        private static List<KpiCardDto> BuildKpis(IReadOnlyDictionary<string, ExecutiveMeasure> measures)
        {
            var kpis = new List<KpiCardDto>();

            void Add(string code, string variant, Func<ExecutiveMeasure, string?> description)
            {
                if (!measures.TryGetValue(code, out var m))
                {
                    return;
                }

                kpis.Add(new KpiCardDto
                {
                    Code = code,
                    Title = m.MetricName,
                    Value = m.ScorePercent ?? 0m,
                    Pending = string.Equals(m.Status, "Pending", StringComparison.OrdinalIgnoreCase),
                    Desc = description(m),
                    Variant = variant,
                    Target = m.TargetPercent.HasValue ? $"TARGET: {Trim(m.TargetPercent.Value)}%" : null
                });
            }

            Add("PSEC", "green", m => $"{Trim(m.Numerator)} of {Trim(m.Denominator)} Priority SIF Exposures Verified");
            Add("CCVC", "blue", m => $"{Trim(m.Numerator)} of {Trim(m.Denominator)} Critical Safeguards Applicable Verified");
            Add("PSIE", "purple", m => m.Notes);

            return kpis;
        }

        private static ConformanceDto BuildConformance(ExecutiveMeasure? conf) => new()
        {
            Value = conf?.ScorePercent ?? 0m,
            Target = conf?.TargetPercent.HasValue == true ? $"TARGET: {Trim(conf.TargetPercent.Value)}%" : null,
            Bands = new List<ConformanceBandDto>
            {
                new() { Status = "failed", From = 0, To = 50 },
                new() { Status = "degraded", From = 50, To = 80 },
                new() { Status = "effective", From = 80, To = 100 }
            }
        };

        private static HealthMapDto BuildHealthMap(IEnumerable<ClsrHealthMapRow> rows) => new()
        {
            Zones = new List<string> { "Z11", "Z12", "Z13", "Z14" },
            Rows = rows.Select(r =>
            {
                var cells = new[]
                {
                    (r.Zona11Status, r.Zona11Score),
                    (r.Zona12Status, r.Zona12Score),
                    (r.Zona13Status, r.Zona13Score),
                    (r.Zona14Status, r.Zona14Score)
                }.Select(z =>
                {
                    var status = MapStatus(z.Item1);
                    return new HealthMapCellDto
                    {
                        Status = status,
                        Score = z.Item2,
                        // Angka hanya dicetak di sel berstatus "failed", mengikuti desain dashboard.
                        Value = status == "failed" ? z.Item2 : null
                    };
                }).ToList();

                return new HealthMapRowDto
                {
                    Name = r.ClsrDescription,
                    Cells = cells,
                    Regional = r.Regional4Score,
                    RegionalStatus = MapStatus(r.HealthStatus)
                };
            }).ToList()
        };

        private static string MapStatus(string? excelStatus)
            => excelStatus is not null && StatusKey.TryGetValue(excelStatus, out var mapped) ? mapped : "nodata";

        private static List<TopPanelDto> BuildTopPanels(IReadOnlyList<TopFiveItem> allItems)
        {
            var panels = new List<TopPanelDto>();

            foreach (var cfg in TopPanelConfig)
            {
                var rows = allItems
                    .Where(x => string.Equals(x.Category, cfg.Category, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (rows.Count == 0)
                {
                    continue;
                }

                // Pembagi minimal 1 supaya panel tanpa data tidak memicu pembagian nol.
                var maxCount = Math.Max(rows.Max(x => x.Count), 1);

                panels.Add(new TopPanelDto
                {
                    No = cfg.No,
                    Title = cfg.Title,
                    Subtitle = cfg.Subtitle,
                    Variant = cfg.Variant,
                    Dash = cfg.Dash,
                    Items = rows.Select(x => new TopPanelItemDto
                    {
                        Label = x.Item,
                        Display = cfg.WithPercent
                            ? FormattableString.Invariant(
                                $"{x.Count} ({Math.Round((x.Percent ?? 0m) * 100, MidpointRounding.AwayFromZero)}%)")
                            : x.Count.ToString(CultureInfo.InvariantCulture),
                        Weight = Math.Round(x.Count / (decimal)maxCount, 6)
                    }).ToList(),
                    Footer = new TopPanelFooterDto
                    {
                        Icon = cfg.FooterIcon,
                        Label = cfg.FooterLabel,
                        Value = rows[0].Denominator ?? 0
                    }
                });
            }

            return panels;
        }

        private static TrendDto BuildTrend(IReadOnlyList<TrendPoint> points, decimal target) => new()
        {
            Target = target,
            TargetLabel = $"Target: {Trim(target)}%",
            Points = points
                .Where(x => !x.IsProjection && x.ActualPercent.HasValue)
                .Select(x => new TrendPointDto { Month = x.MonthLabel, Value = x.ActualPercent!.Value })
                .ToList(),
            Projection = points
                .Where(x => x.IsProjection && x.PlannedPercent.HasValue)
                .Select(x => new TrendPointDto { Month = x.MonthLabel, Value = x.PlannedPercent!.Value })
                .ToList()
        };

        private static ZonaScoresDto BuildZonaScores(IReadOnlyList<ZonaScore> scores, decimal target) => new()
        {
            Target = target,
            TargetLabel = $"Target: {Trim(target)}%",
            Bars = scores.Select(x => new ZonaBarDto
            {
                Zone = x.ZonaLabel ?? $"Zona {x.Zona}",
                Obs = x.ObservationCount,
                Value = x.ScorePercent
            }).ToList()
        };

        private static List<SummaryCardDto> BuildSummaryCards(
            IReadOnlyDictionary<string, string> texts,
            IReadOnlyList<TopFiveItem> topItems)
        {
            var topSystemic = topItems
                .Where(x => string.Equals(x.Category, "Top Systemic Issue", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.DisplayOrder)
                .Take(3)
                .Select(x => x.Item)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return new List<SummaryCardDto>
            {
                new() { Icon = "warning", Tone = "red", Title = "TOP SIF EXPOSURES", Text = texts.GetValueOrDefault("Top SIF Exposure Note") },
                new() { Icon = "shield", Tone = "navy", Title = "CRITICAL GAPS", Text = texts.GetValueOrDefault("Critical Gaps Note") },
                new() { Icon = "pin", Tone = "red", Title = "ZONA ATTENTION", Text = texts.GetValueOrDefault("Zona Attention Note") },
                new()
                {
                    Icon = "people",
                    Tone = "navy",
                    Title = "KEY SYSTEMIC ISSUE",
                    Text = topSystemic.Count > 0
                        ? $"{string.Join(", ", topSystemic)} menjadi systemic issue utama yang perlu ditangani segera."
                        : null
                },
                new() { Icon = "target", Tone = "green", Title = "FOCUS AREA", Text = texts.GetValueOrDefault("Focus Area Note") }
            };
        }

        private static List<string> BuildSummaryNotes(
            IReadOnlyDictionary<string, string> texts,
            IReadOnlyList<QuickFact> quickFacts)
        {
            string Fact(string label) => quickFacts
                .FirstOrDefault(f => string.Equals(f.FactName, label, StringComparison.OrdinalIgnoreCase))?.FactValue ?? "-";

            return new List<string>
            {
                $"Data berdasarkan {Fact("Total Observations Completed")} observasi pada periode {Fact("Observation Period")}.",
                $"Data cutoff: {texts.GetValueOrDefault("Data Cutoff", "-")}.",
                "Dashboard diperbarui setiap bulan."
            };
        }

        /// <summary>
        /// Membuang nol di belakang koma supaya "80.0000" tampil sebagai "80".
        /// InvariantCulture dipakai karena hasilnya masuk ke payload API (dibaca klien),
        /// bukan teks yang diformat mengikuti locale server.
        /// </summary>
        private static string Trim(decimal? value)
            => value.HasValue ? value.Value.Normalize().ToString("0.####", CultureInfo.InvariantCulture) : "-";

        public async Task<AdminDashboardSummaryDto> GetAdminSummaryAsync(CancellationToken cancellationToken = default)
        {
            var lastBatch = await _context.ImportBatches.AsNoTracking()
                .Where(x => x.Status == ImportStatus.Completed)
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var conf = await _context.ExecutiveMeasures.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MetricCode == "CONF", cancellationToken);

            return new AdminDashboardSummaryDto
            {
                TotalObservations = await _context.Observations.CountAsync(cancellationToken),
                ActiveObservations = await _context.Observations.CountAsync(x => x.IsActive, cancellationToken),
                TotalSifQuestions = await _context.SifQuestions.CountAsync(cancellationToken),
                TotalFindings = await _context.ErrorTraps.CountAsync(cancellationToken)
                                + await _context.DriftConditions.CountAsync(cancellationToken)
                                + await _context.LatentConditions.CountAsync(cancellationToken),
                TotalInitiatives = await _context.ImprovementInitiatives.CountAsync(cancellationToken),
                OpenInitiatives = await _context.ImprovementInitiatives
                    .CountAsync(x => x.Status != null && x.Status != "Completed", cancellationToken),
                ConformanceScore = conf?.ScorePercent,
                ZonesCovered = await _context.Observations
                    .Where(x => x.Zona != null)
                    .Select(x => x.Zona)
                    .Distinct()
                    .CountAsync(cancellationToken),
                LastImportAt = lastBatch?.CompletedAt ?? lastBatch?.CreatedAt,
                LastImportFile = lastBatch?.FileName,
                ZonaBreakdown = await _context.ZonaScores.AsNoTracking()
                    .OrderBy(x => x.Zona)
                    .Select(x => new ZonaBarDto
                    {
                        Zone = x.ZonaLabel ?? "Zona " + x.Zona,
                        Obs = x.ObservationCount,
                        Value = x.ScorePercent
                    })
                    .ToListAsync(cancellationToken)
            };
        }
    }

    internal static class DecimalExtensions
    {
        /// <summary>Menghapus trailing zero pada decimal (80.0000m → 80m).</summary>
        public static decimal Normalize(this decimal value) => value / 1.000000000000000000000000000000000m;
    }
}
