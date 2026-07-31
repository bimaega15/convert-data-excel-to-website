using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Services.Mappers;

namespace Sifp_Vue.Server.Data.Seeders
{
    /// <summary>
    /// Mengisi master data awal dari hasil konversi Excel yang sudah ada di proyek Vue
    /// (<c>src/data/generated</c>), sehingga database langsung berisi data nyata tanpa
    /// perlu upload workbook lebih dulu. Dilewati bila tabel Observations sudah terisi.
    /// </summary>
    public class MasterDataSeeder : IDataSeeder
    {
        public int Order => 2;
        public string Name => nameof(MasterDataSeeder);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            // Kolom teks di workbook kadang berisi angka (mis. "company": 0),
            // jadi seluruh properti string dibaca lewat konverter yang toleran.
            Converters = { new FlexibleStringConverter() }
        };

        private static readonly string[] MonthNames =
            { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        /// <summary>Kebalikan pemetaan status di DashboardService, untuk mengembalikan bentuk aslinya.</summary>
        private static readonly Dictionary<string, string> StatusLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["effective"] = "Effective",
            ["degraded"] = "Degraded",
            ["failed"] = "Failed / High Concern",
            ["nodata"] = "No Data"
        };

        /// <summary>Nomor panel Top 5 di dashboard.json → nama kategori di tabel TopFiveItems.</summary>
        private static readonly Dictionary<int, string> TopPanelCategories = new()
        {
            [2] = "Top SIF Exposure",
            [3] = "Top Critical Safeguard Gap",
            [4] = "Top Recurring Drift",
            [5] = "Top Systemic Issue"
        };

        private readonly SifpDbContext _context;
        private readonly SeedOptions _options;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MasterDataSeeder> _logger;

        public MasterDataSeeder(
            SifpDbContext context,
            IOptions<SeedOptions> options,
            IWebHostEnvironment environment,
            ILogger<MasterDataSeeder> logger)
        {
            _context = context;
            _options = options.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.SeedSampleData)
            {
                return;
            }

            if (await _context.Observations.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Master data sudah terisi, seeding dilewati.");
                return;
            }

            var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.GeneratedDataPath));
            if (!Directory.Exists(root))
            {
                _logger.LogWarning(
                    "Folder data hasil konversi tidak ditemukan di {Path}. " +
                    "Jalankan `npm run convert:excel` di sifp_vue.client, atau import workbook lewat /api/import/excel.",
                    root);
                return;
            }

            var dashboard = ReadJson<SeedDashboard>(Path.Combine(root, "dashboard.json"));

            var batch = new ImportBatch
            {
                FileName = dashboard?.Meta?.SourceFile ?? "seed-data.xlsx",
                FileSizeBytes = 0,
                Status = ImportStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                CreatedBy = "SEEDER",
                SummaryJson = null
            };

            _context.ImportBatches.Add(batch);
            await _context.SaveChangesAsync(cancellationToken);

            var observationIds = await SeedObservationsAsync(root, batch.Id, cancellationToken);
            if (observationIds.Count == 0)
            {
                _logger.LogWarning("observations.json kosong atau tidak ada; seeding master data dihentikan.");
                return;
            }

            SeedSifQuestions(root, batch.Id, observationIds);
            SeedErrorTraps(root, batch.Id, observationIds);
            SeedHpTools(root, batch.Id, observationIds);
            SeedDriftConditions(root, batch.Id, observationIds);
            SeedLatentConditions(root, batch.Id, observationIds);
            SeedCcvcLibrary(root, batch.Id);
            SeedInitiatives(root, batch.Id);

            if (dashboard is not null)
            {
                SeedDashboardTables(dashboard, batch.Id);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var sheetCount = await SeedWorksheetsAsync(root, batch.Id, cancellationToken);

            batch.SheetCount = sheetCount;
            batch.TotalRows = observationIds.Count;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Master data awal ter-seed: {Observations} observasi, {Sheets} worksheet (sumber: {File})",
                observationIds.Count, sheetCount, batch.FileName);
        }

        private async Task<Dictionary<string, int>> SeedObservationsAsync(string root, int batchId, CancellationToken cancellationToken)
        {
            var rows = ReadJson<List<SeedObservation>>(Path.Combine(root, "observations.json")) ?? new();

            var entities = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Id))
                .Select(r =>
                {
                    var observers = r.Observers ?? new List<string>();
                    return new Observation
                    {
                        ObsCode = r.Id!,
                        ProtocolCode = r.ProtocolCode,
                        ProtocolName = r.ProtocolName,
                        ObservationDate = ParseDate(r.Date),
                        Zona = r.Zona,
                        Site = r.Site,
                        AreaEquipment = r.Area,
                        Activity = r.Activity,
                        Company = r.Company,
                        Observer1 = observers.ElementAtOrDefault(0),
                        Observer2 = observers.ElementAtOrDefault(1),
                        Observer3 = observers.ElementAtOrDefault(2),
                        YesCount = r.Yes,
                        NoCount = r.No,
                        NaCount = r.Na,
                        PerformancePercent = r.Performance,
                        ObservationSequence = r.Sequence,
                        PsieEligible = MasterDataMapper.FromYesNo(r.PsieEligible),
                        Status = r.Status,
                        IsActive = MasterDataMapper.FromYesNo(r.Active),
                        ImportBatchId = batchId,
                        CreatedBy = "SEEDER"
                    };
                })
                .ToList();

            _context.Observations.AddRange(entities);
            await _context.SaveChangesAsync(cancellationToken);

            return entities.ToDictionary(x => x.ObsCode, x => x.Id);
        }

        private void SeedSifQuestions(string root, int batchId, IReadOnlyDictionary<string, int> obsIds)
        {
            var rows = ReadJson<List<SeedSifQuestion>>(Path.Combine(root, "sif-questions.json")) ?? new();

            _context.SifQuestions.AddRange(rows
                .Where(r => r.ObsId is not null && obsIds.ContainsKey(r.ObsId))
                .Select(r => new SifQuestion
                {
                    ObservationId = obsIds[r.ObsId!],
                    ProtocolCode = r.ProtocolCode,
                    ProtocolName = r.ProtocolName,
                    QuestionRef = r.QuestionRef,
                    CcvcId = r.CcvcId,
                    QuestionText = ExcelCellReader.Truncate(r.Question, 1000),
                    Answer = r.Answer ?? "-",
                    Comments = r.Comments,
                    SifExposure = r.SifExposure,
                    CriticalSafeguard = r.CriticalSafeguard,
                    ObservationDate = ParseDate(r.Date),
                    Zona = r.Zona,
                    Site = r.Site,
                    Activity = r.Activity,
                    Company = r.Company,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        private void SeedErrorTraps(string root, int batchId, IReadOnlyDictionary<string, int> obsIds)
        {
            var rows = ReadJson<List<SeedErrorTrap>>(Path.Combine(root, "error-traps.json")) ?? new();

            _context.ErrorTraps.AddRange(rows
                .Where(r => r.ObsId is not null && obsIds.ContainsKey(r.ObsId))
                .Select(r => new ErrorTrap
                {
                    ObservationId = obsIds[r.ObsId!],
                    ProtocolCode = r.ProtocolCode,
                    ProtocolName = r.ProtocolName,
                    Category = r.Category,
                    TrapName = r.ErrorTrap,
                    Comments = r.Comments,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        private void SeedHpTools(string root, int batchId, IReadOnlyDictionary<string, int> obsIds)
        {
            var rows = ReadJson<List<SeedHpTool>>(Path.Combine(root, "hp-tools.json")) ?? new();

            _context.HpTools.AddRange(rows
                .Where(r => r.ObsId is not null && obsIds.ContainsKey(r.ObsId))
                .Select(r => new HpTool
                {
                    ObservationId = obsIds[r.ObsId!],
                    ProtocolCode = r.ProtocolCode,
                    ProtocolName = r.ProtocolName,
                    ToolName = r.Tool,
                    Tujuan = r.Tujuan,
                    KapanDigunakan = r.KapanDigunakan,
                    CaraPakai = r.CaraPakai,
                    EffectivenessNotes = r.EffectivenessNotes,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        private void SeedDriftConditions(string root, int batchId, IReadOnlyDictionary<string, int> obsIds)
        {
            var rows = ReadJson<List<SeedDriftCondition>>(Path.Combine(root, "drift-conditions.json")) ?? new();

            _context.DriftConditions.AddRange(rows
                .Where(r => r.ObsId is not null && obsIds.ContainsKey(r.ObsId))
                .Select(r => new DriftCondition
                {
                    ObservationId = obsIds[r.ObsId!],
                    ProtocolCode = r.ProtocolCode,
                    ProtocolName = r.ProtocolName,
                    Situation = r.Situation,
                    Level1 = r.Level1,
                    Code = r.Code,
                    Level2 = r.Level2,
                    Reason = r.Reason,
                    Sequence = r.Sequence,
                    Status = r.Status,
                    IsActive = MasterDataMapper.FromYesNo(r.Active),
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        private void SeedLatentConditions(string root, int batchId, IReadOnlyDictionary<string, int> obsIds)
        {
            var rows = ReadJson<List<SeedLatentCondition>>(Path.Combine(root, "latent-conditions.json")) ?? new();

            _context.LatentConditions.AddRange(rows
                .Where(r => r.ObsId is not null && obsIds.ContainsKey(r.ObsId))
                .Select(r => new LatentCondition
                {
                    ObservationId = obsIds[r.ObsId!],
                    ProtocolCode = r.ProtocolCode,
                    ProtocolName = r.ProtocolName,
                    ObservationText = r.Observation,
                    Level1 = r.Level1,
                    Code = r.Code,
                    Level2 = r.Level2,
                    Reason = r.Reason,
                    Sequence = r.Sequence,
                    Status = r.Status,
                    IsActive = MasterDataMapper.FromYesNo(r.Active),
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        private void SeedCcvcLibrary(string root, int batchId)
        {
            var rows = ReadJson<List<SeedCcvcItem>>(Path.Combine(root, "ccvc-library.json")) ?? new();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            _context.CcvcLibraryItems.AddRange(rows
                .Where(r => !string.IsNullOrWhiteSpace(r.CcvcId) && seen.Add(r.CcvcId!))
                .Select(r => new CcvcLibraryItem
                {
                    RowNo = r.No,
                    ProtocolGroup = r.ProtocolGroup,
                    PsecId = r.PsecId,
                    PsecName = r.PsecName,
                    ExposureType = r.ExposureType,
                    CcvcId = r.CcvcId!,
                    QuestionCode = r.QuestionCode,
                    QuestionSummary = ExcelCellReader.Truncate(r.QuestionSummary, 500),
                    VerificationPurpose = r.VerificationPurpose,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        private void SeedInitiatives(string root, int batchId)
        {
            var rows = ReadJson<List<SeedInitiative>>(Path.Combine(root, "initiatives.json")) ?? new();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            _context.ImprovementInitiatives.AddRange(rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Id) && seen.Add(r.Id!))
                .Select(r => new ImprovementInitiative
                {
                    ImprovementCode = r.Id!,
                    Initiative = r.Initiative,
                    RelatedClsr = r.RelatedClsr,
                    Owner = r.Owner,
                    Status = r.Status,
                    ProgressPercent = r.Progress ?? 0,
                    ExpectedImpact = r.ExpectedImpact,
                    Notes = r.Notes,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                }));
        }

        /// <summary>
        /// dashboard.json adalah bentuk turunan, jadi sebagian nilai (numerator,
        /// denominator, count) diambil kembali dari teks tampilannya.
        /// </summary>
        private void SeedDashboardTables(SeedDashboard dashboard, int batchId)
        {
            foreach (var kpi in dashboard.Kpis ?? new List<SeedKpi>())
            {
                if (string.IsNullOrWhiteSpace(kpi.Code))
                {
                    continue;
                }

                var (numerator, denominator) = ParseFraction(kpi.Desc);

                _context.ExecutiveMeasures.Add(new ExecutiveMeasure
                {
                    MetricCode = kpi.Code!,
                    MetricName = kpi.Title,
                    Numerator = numerator,
                    Denominator = denominator,
                    ScorePercent = kpi.Value,
                    TargetPercent = ParseTrailingPercent(kpi.Target),
                    Status = kpi.Pending ? "Pending" : "Active",
                    Notes = kpi.Pending ? kpi.Desc : null,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            if (dashboard.Conformance is not null)
            {
                _context.ExecutiveMeasures.Add(new ExecutiveMeasure
                {
                    MetricCode = "CONF",
                    MetricName = "Regional 4 Conformance Score",
                    ScorePercent = dashboard.Conformance.Value,
                    TargetPercent = ParseTrailingPercent(dashboard.Conformance.Target),
                    Status = "Active",
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            var factOrder = 0;
            foreach (var fact in dashboard.QuickFacts ?? new List<SeedQuickFact>())
            {
                if (string.IsNullOrWhiteSpace(fact.Label))
                {
                    continue;
                }

                _context.QuickFacts.Add(new QuickFact
                {
                    FactName = fact.Label!,
                    FactValue = fact.Value,
                    Icon = fact.Icon,
                    DisplayOrder = factOrder++,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            var healthOrder = 0;
            foreach (var row in dashboard.HealthMap?.Rows ?? new List<SeedHealthRow>())
            {
                var cells = row.Cells ?? new List<SeedHealthCell>();

                _context.ClsrHealthMapRows.Add(new ClsrHealthMapRow
                {
                    // dashboard.json hanya memuat deskripsi, jadi ID dibangkitkan berurutan.
                    ClsrId = $"CLSR{healthOrder + 1:D2}",
                    ClsrDescription = row.Name,
                    Zona11Status = StatusLabel(cells.ElementAtOrDefault(0)?.Status),
                    Zona11Score = cells.ElementAtOrDefault(0)?.Score,
                    Zona12Status = StatusLabel(cells.ElementAtOrDefault(1)?.Status),
                    Zona12Score = cells.ElementAtOrDefault(1)?.Score,
                    Zona13Status = StatusLabel(cells.ElementAtOrDefault(2)?.Status),
                    Zona13Score = cells.ElementAtOrDefault(2)?.Score,
                    Zona14Status = StatusLabel(cells.ElementAtOrDefault(3)?.Status),
                    Zona14Score = cells.ElementAtOrDefault(3)?.Score,
                    Regional4Score = row.Regional,
                    HealthStatus = StatusLabel(row.RegionalStatus),
                    DisplayOrder = healthOrder++,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            var topOrder = 0;
            foreach (var panel in dashboard.TopPanels ?? new List<SeedTopPanel>())
            {
                if (!TopPanelCategories.TryGetValue(panel.No, out var category))
                {
                    continue;
                }

                var denominator = panel.Footer?.Value ?? 0;

                foreach (var item in panel.Items ?? new List<SeedTopItem>())
                {
                    var (count, percent) = ParseDisplay(item.Display, denominator);

                    _context.TopFiveItems.Add(new TopFiveItem
                    {
                        Category = category,
                        Item = item.Label,
                        Count = count,
                        Percent = percent,
                        Denominator = denominator,
                        DisplayOrder = topOrder++,
                        ImportBatchId = batchId,
                        CreatedBy = "SEEDER"
                    });
                }
            }

            var trendOrder = 0;
            var seenMonths = new HashSet<DateOnly>();

            void AddTrend(SeedTrendPoint point, bool isProjection)
            {
                var month = ParseMonthLabel(point.Month);
                if (!month.HasValue || !seenMonths.Add(month.Value))
                {
                    return;
                }

                _context.TrendPoints.Add(new TrendPoint
                {
                    PeriodMonth = month.Value,
                    MonthLabel = point.Month,
                    ActualPercent = isProjection ? null : point.Value,
                    PlannedPercent = isProjection ? point.Value : null,
                    ObservationCount = isProjection ? 0 : 1,
                    IsProjection = isProjection,
                    DisplayOrder = trendOrder++,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            foreach (var point in dashboard.Trend?.Points ?? new List<SeedTrendPoint>()) AddTrend(point, false);
            foreach (var point in dashboard.Trend?.Projection ?? new List<SeedTrendPoint>()) AddTrend(point, true);

            var zonaOrder = 0;
            var seenZonas = new HashSet<int>();

            foreach (var bar in dashboard.ZonaScores?.Bars ?? new List<SeedZonaBar>())
            {
                var zona = ParseLeadingInt(bar.Zone);
                if (!zona.HasValue || !seenZonas.Add(zona.Value))
                {
                    continue;
                }

                _context.ZonaScores.Add(new ZonaScore
                {
                    Zona = zona.Value,
                    ZonaLabel = bar.Zone,
                    ScorePercent = bar.Value,
                    ObservationCount = bar.Obs,
                    DisplayOrder = zonaOrder++,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            SeedDashboardTexts(dashboard, batchId);
        }

        private void SeedDashboardTexts(SeedDashboard dashboard, int batchId)
        {
            // Judul kartu ringkasan → nama Section pada sheet CONFIG-DASHBOARD_TEXT.
            var sectionByTitle = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["TOP SIF EXPOSURES"] = "Top SIF Exposure Note",
                ["CRITICAL GAPS"] = "Critical Gaps Note",
                ["ZONA ATTENTION"] = "Zona Attention Note",
                ["FOCUS AREA"] = "Focus Area Note"
            };

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var card in dashboard.SummaryCards ?? new List<SeedSummaryCard>())
            {
                if (card.Title is null ||
                    !sectionByTitle.TryGetValue(card.Title, out var section) ||
                    string.IsNullOrWhiteSpace(card.Text) ||
                    !seen.Add(section))
                {
                    continue;
                }

                _context.DashboardTexts.Add(new DashboardText
                {
                    Section = section,
                    Text = card.Text,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }

            var cutoffNote = (dashboard.SummaryNotes ?? new List<string>())
                .FirstOrDefault(n => n.StartsWith("Data cutoff:", StringComparison.OrdinalIgnoreCase));

            if (cutoffNote is not null && seen.Add("Data Cutoff"))
            {
                _context.DashboardTexts.Add(new DashboardText
                {
                    Section = "Data Cutoff",
                    Text = cutoffNote["Data cutoff:".Length..].Trim().TrimEnd('.'),
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                });
            }
        }

        private async Task<int> SeedWorksheetsAsync(string root, int batchId, CancellationToken cancellationToken)
        {
            var sheetsDir = Path.Combine(root, "sheets");
            var manifest = ReadJson<SeedManifest>(Path.Combine(sheetsDir, "_manifest.json"));
            if (manifest?.Groups is null)
            {
                return 0;
            }

            var count = 0;

            foreach (var item in manifest.Groups.SelectMany(g => g.Items ?? new List<SeedManifestItem>()))
            {
                if (string.IsNullOrWhiteSpace(item.Slug) || string.IsNullOrWhiteSpace(item.Name))
                {
                    continue;
                }

                var data = ReadJson<SeedSheetData>(Path.Combine(sheetsDir, $"{item.Slug}.json"));
                var rows = data?.Rows ?? new List<string[]>();

                var worksheet = new Worksheet
                {
                    Name = item.Name!,
                    Slug = item.Slug!,
                    SheetIndex = item.Index,
                    GroupName = item.Group,
                    Label = item.Label,
                    Icon = item.Icon,
                    Route = item.Route,
                    IsCurated = item.Curated,
                    IsRequired = SheetSchema.RequiredSheetNames.Contains(item.Name!),
                    RowCount = rows.Count,
                    ColCount = data?.ColCount ?? item.ColCount,
                    ImportBatchId = batchId,
                    CreatedBy = "SEEDER"
                };

                _context.Worksheets.Add(worksheet);
                await _context.SaveChangesAsync(cancellationToken);

                var rowIndex = 0;
                _context.WorksheetRows.AddRange(rows.Select(cells => new WorksheetRow
                {
                    WorksheetId = worksheet.Id,
                    // File JSON tidak menyimpan nomor baris asli, jadi dipakai urutan barisnya.
                    ExcelRow = rowIndex + 1,
                    RowIndex = rowIndex++,
                    CellsJson = JsonSerializer.Serialize(cells)
                }));

                await _context.SaveChangesAsync(cancellationToken);
                _context.ChangeTracker.Clear();
                count++;
            }

            return count;
        }

        // ---------- Utilitas parsing ----------

        private T? ReadJson<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                _logger.LogWarning("File seed tidak ditemukan: {Path}", path);
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "File seed {Path} bukan JSON yang valid", path);
                return null;
            }
        }

        private static DateOnly? ParseDate(string? iso)
            => DateOnly.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

        /// <summary>"May-26" → 2026-05-01.</summary>
        private static DateOnly? ParseMonthLabel(string? label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return null;
            }

            var parts = label.Split('-', 2);
            if (parts.Length != 2)
            {
                return null;
            }

            var monthIndex = Array.FindIndex(MonthNames, m => m.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
            if (monthIndex < 0 || !int.TryParse(parts[1], out var shortYear))
            {
                return null;
            }

            return new DateOnly(2000 + shortYear, monthIndex + 1, 1);
        }

        private static string? StatusLabel(string? key)
            => key is not null && StatusLabels.TryGetValue(key, out var label) ? label : null;

        /// <summary>"18 of 23 Priority SIF Exposures Verified" → (18, 23).</summary>
        private static (decimal? Numerator, decimal? Denominator) ParseFraction(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (null, null);
            }

            var match = Regex.Match(text, @"^\s*(\d+(?:[.,]\d+)?)\s+of\s+(\d+(?:[.,]\d+)?)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return (null, null);
            }

            return (ToDecimal(match.Groups[1].Value), ToDecimal(match.Groups[2].Value));
        }

        /// <summary>"TARGET: 80%" → 80.</summary>
        private static decimal? ParseTrailingPercent(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = Regex.Match(text, @"(\d+(?:[.,]\d+)?)\s*%");
            return match.Success ? ToDecimal(match.Groups[1].Value) : null;
        }

        /// <summary>"12 (52%)" → count 12, percent 0.52. "12" → count 12, percent dari denominator.</summary>
        private static (int Count, decimal? Percent) ParseDisplay(string? display, int denominator)
        {
            if (string.IsNullOrWhiteSpace(display))
            {
                return (0, null);
            }

            var countMatch = Regex.Match(display, @"^\s*(\d+)");
            var count = countMatch.Success ? int.Parse(countMatch.Groups[1].Value) : 0;

            var percentMatch = Regex.Match(display, @"\((\d+(?:[.,]\d+)?)\s*%\)");
            if (percentMatch.Success)
            {
                var percent = ToDecimal(percentMatch.Groups[1].Value);
                return (count, percent.HasValue ? percent.Value / 100m : null);
            }

            return (count, denominator > 0 ? Math.Round(count / (decimal)denominator, 6) : null);
        }

        private static int? ParseLeadingInt(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = Regex.Match(text, @"(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : null;
        }

        private static decimal? ToDecimal(string raw)
            => decimal.TryParse(raw.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
    }
}
