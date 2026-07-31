using System.Security.Cryptography;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Sifp_Vue.Server.Data;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Models.Entities;
using Sifp_Vue.Server.Services.Contracts;
using Sifp_Vue.Server.Services.Mappers;
using static Sifp_Vue.Server.Helpers.ExcelCellReader;

namespace Sifp_Vue.Server.Services
{
    public class ImportOptions
    {
        public const string SectionName = "Import";

        public int MaxFileSizeMb { get; set; } = 25;

        /// <summary>ClosedXML hanya membaca format OpenXML; .xls (BIFF lama) tidak didukung.</summary>
        public string[] AllowedExtensions { get; set; } = { ".xlsx", ".xlsm" };
    }

    /// <summary>
    /// Menggantikan peran <c>scripts/convert-excel.mjs</c>: workbook diparse di server
    /// dan hasilnya disimpan ke SQL Server, bukan ditulis sebagai file JSON.
    /// Aturan pemetaan kolom mengikuti converter tersebut baris demi baris.
    /// </summary>
    public class ExcelImportService : IExcelImportService
    {
        private static readonly string[] MonthNames =
            { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private static readonly Dictionary<string, string> QuickFactIcons = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Total Observations Completed"] = "clipboard",
            ["Priority SIF Exposure Verified (PSEC)"] = "shield",
            ["Critical Safeguards Verified (CCVC)"] = "checklist",
            ["Regional 4 Conformance Score"] = "gear",
            ["Zones Covered"] = "pin",
            ["Observation Period"] = "calendar",
            ["Sites / Locations Observed"] = "pin",
            ["Missing Zones"] = "warning",
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly SifpDbContext _context;
        private readonly ImportOptions _options;
        private readonly ILogger<ExcelImportService> _logger;

        public ExcelImportService(
            SifpDbContext context,
            Microsoft.Extensions.Options.IOptions<ImportOptions> options,
            ILogger<ExcelImportService> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<ImportResultDto>> ImportAsync(
            Stream fileStream,
            string fileName,
            string? summaryJson,
            string? editsJson,
            string actor,
            CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(extension))
            {
                return ApiResponse<ImportResultDto>.Fail(
                    $"Format \"{extension}\" tidak didukung. Gunakan {string.Join(" atau ", _options.AllowedExtensions)}.");
            }

            // Stream disalin ke memori: ClosedXML butuh stream yang bisa di-seek,
            // dan hash file dihitung dari byte yang sama persis dengan yang diparse.
            using var buffer = new MemoryStream();
            await fileStream.CopyToAsync(buffer, cancellationToken);

            if (buffer.Length == 0)
            {
                return ApiResponse<ImportResultDto>.Fail("File kosong (0 byte).");
            }

            var maxBytes = (long)_options.MaxFileSizeMb * 1024 * 1024;
            if (buffer.Length > maxBytes)
            {
                return ApiResponse<ImportResultDto>.Fail(
                    $"Ukuran file melebihi batas {_options.MaxFileSizeMb} MB.");
            }

            buffer.Position = 0;
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(buffer, cancellationToken)).ToLowerInvariant();
            buffer.Position = 0;

            var edits = Deserialize<List<CellEditDto>>(editsJson) ?? new List<CellEditDto>();
            var summary = Deserialize<ImportSummaryDto>(summaryJson);

            var batch = new ImportBatch
            {
                FileName = Truncate(fileName, 400)!,
                FileSizeBytes = buffer.Length,
                FileHash = hash,
                Status = ImportStatus.Processing,
                EditCount = edits.Count,
                EditsJson = editsJson,
                SummaryJson = summaryJson,
                SheetCount = summary?.SheetCount ?? 0,
                TotalRows = summary?.TotalRows ?? 0,
                CreatedBy = actor
            };

            _context.ImportBatches.Add(batch);
            await _context.SaveChangesAsync(cancellationToken);

            var warnings = new List<string>();

            try
            {
                using var workbook = new WorkbookAccessor(buffer);

                var missing = SheetSchema.FindMissingSheets(workbook.SheetNames);
                if (missing.Count > 0)
                {
                    var detail = string.Join(", ", missing.Select(m => $"{m.Name} ({m.Label})"));
                    throw new InvalidOperationException($"{missing.Count} sheet wajib tidak ditemukan: {detail}");
                }

                var editsApplied = workbook.ApplyEdits(edits, warnings);
                var editWarningCount = warnings.Count;
                var batchId = batch.Id;

                // Retry bawaan SqlServer melarang transaksi yang dibuka manual, kecuali
                // seluruh operasinya dibungkus execution strategy sebagai satu unit
                // yang bisa diulang. Delegasi di bawah karenanya harus aman dijalankan ulang.
                var strategy = _context.Database.CreateExecutionStrategy();

                var counts = await strategy.ExecuteAsync(async () =>
                {
                    // Bersihkan sisa percobaan sebelumnya bila strategy mengulang.
                    _context.ChangeTracker.Clear();
                    while (warnings.Count > editWarningCount)
                    {
                        warnings.RemoveAt(warnings.Count - 1);
                    }

                    // Transaksi menjamin data lama tidak pernah terhapus tanpa data baru
                    // yang menggantikannya bila parsing gagal di tengah jalan.
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                    await ClearExistingDataAsync(cancellationToken);

                    var result = new Dictionary<string, int>
                    {
                        ["Observations"] = await ImportObservationsAsync(workbook, batchId, cancellationToken)
                    };

                    var observationIds = await _context.Observations
                        .AsNoTracking()
                        .ToDictionaryAsync(x => x.ObsCode, x => x.Id, cancellationToken);

                    result["SifQuestions"] = ImportSifQuestions(workbook, batchId, observationIds, warnings);
                    result["ErrorTraps"] = ImportErrorTraps(workbook, batchId, observationIds, warnings);
                    result["HpTools"] = ImportHpTools(workbook, batchId, observationIds, warnings);
                    result["DriftConditions"] = ImportDriftConditions(workbook, batchId, observationIds, warnings);
                    result["LatentConditions"] = ImportLatentConditions(workbook, batchId, observationIds, warnings);
                    result["CcvcLibraryItems"] = ImportCcvcLibrary(workbook, batchId);
                    result["ImprovementInitiatives"] = ImportInitiatives(workbook, batchId);
                    result["ExecutiveMeasures"] = ImportExecutiveMeasures(workbook, batchId);
                    result["QuickFacts"] = ImportQuickFacts(workbook, batchId);
                    result["ClsrHealthMapRows"] = ImportHealthMap(workbook, batchId);
                    result["TopFiveItems"] = ImportTopFive(workbook, batchId);
                    result["DashboardTexts"] = ImportDashboardTexts(workbook, batchId);

                    var (trendCount, zonaCount) = ImportTrendAndZona(workbook, batchId);
                    result["TrendPoints"] = trendCount;
                    result["ZonaScores"] = zonaCount;

                    await _context.SaveChangesAsync(cancellationToken);

                    result["Worksheets"] = await ImportWorksheetsAsync(workbook, batchId, cancellationToken);

                    // ImportWorksheetsAsync mengosongkan change tracker, jadi baris batch
                    // dimuat ulang sebelum statusnya diperbarui.
                    var current = await _context.ImportBatches.FirstAsync(x => x.Id == batchId, cancellationToken);
                    current.Status = ImportStatus.Completed;
                    current.CompletedAt = DateTime.UtcNow;
                    current.SheetCount = result["Worksheets"];
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    return result;
                });

                _logger.LogInformation(
                    "Import batch {BatchId} selesai: {File}, {Sheets} sheet, {Edits} edit diterapkan",
                    batchId, batch.FileName, counts["Worksheets"], editsApplied);

                return ApiResponse<ImportResultDto>.Ok(new ImportResultDto
                {
                    BatchId = batchId,
                    FileName = batch.FileName,
                    Status = ImportStatus.Completed.ToString(),
                    SheetCount = counts["Worksheets"],
                    EditsApplied = editsApplied,
                    CompletedAt = DateTime.UtcNow,
                    RowsImported = counts,
                    Warnings = warnings
                }, "Import berhasil.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import batch {BatchId} gagal untuk file {File}", batch.Id, batch.FileName);

                // Transaksi sudah di-rollback oleh `await using`. Baris batch sendiri
                // sudah tersimpan sebelum transaksi dimulai, jadi status gagal tetap tercatat.
                _context.ChangeTracker.Clear();

                var failed = await _context.ImportBatches.FirstOrDefaultAsync(x => x.Id == batch.Id, cancellationToken);
                if (failed is not null)
                {
                    failed.Status = ImportStatus.Failed;
                    failed.ErrorMessage = ex.Message;
                    failed.CompletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return ApiResponse<ImportResultDto>.Fail($"Import gagal: {ex.Message}");
            }
        }

        private static T? Deserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Menghapus seluruh master data lama. Urutan mengikuti arah foreign key:
        /// tabel anak lebih dulu, baru tabel induk.
        /// </summary>
        private async Task ClearExistingDataAsync(CancellationToken cancellationToken)
        {
            await _context.SifQuestions.ExecuteDeleteAsync(cancellationToken);
            await _context.ErrorTraps.ExecuteDeleteAsync(cancellationToken);
            await _context.HpTools.ExecuteDeleteAsync(cancellationToken);
            await _context.DriftConditions.ExecuteDeleteAsync(cancellationToken);
            await _context.LatentConditions.ExecuteDeleteAsync(cancellationToken);
            await _context.Observations.ExecuteDeleteAsync(cancellationToken);

            await _context.CcvcLibraryItems.ExecuteDeleteAsync(cancellationToken);
            await _context.ImprovementInitiatives.ExecuteDeleteAsync(cancellationToken);
            await _context.ExecutiveMeasures.ExecuteDeleteAsync(cancellationToken);
            await _context.QuickFacts.ExecuteDeleteAsync(cancellationToken);
            await _context.ClsrHealthMapRows.ExecuteDeleteAsync(cancellationToken);
            await _context.TopFiveItems.ExecuteDeleteAsync(cancellationToken);
            await _context.TrendPoints.ExecuteDeleteAsync(cancellationToken);
            await _context.ZonaScores.ExecuteDeleteAsync(cancellationToken);
            await _context.DashboardTexts.ExecuteDeleteAsync(cancellationToken);
        }

        // ---------- Sheet ANALYZE-CONFORMANCE_SCORE ----------

        private async Task<int> ImportObservationsAsync(WorkbookAccessor wb, int batchId, CancellationToken cancellationToken)
        {
            var rows = wb.ObjectRows("ANALYZE-CONFORMANCE_SCORE");
            var entities = new List<Observation>();

            foreach (var r in rows)
            {
                var obsCode = Text(Cell(r, "Obs_ID"));
                if (string.IsNullOrEmpty(obsCode))
                {
                    continue;
                }

                entities.Add(new Observation
                {
                    ObsCode = Truncate(obsCode, 50)!,
                    ProtocolCode = Truncate(Text(Cell(r, "Protocol_Code")), 50),
                    ProtocolName = Truncate(Text(Cell(r, "Protocol_Name")), 200),
                    ObservationDate = Date(Cell(r, "Observation_Date")),
                    Zona = Int(Cell(r, "Zona")),
                    Site = Truncate(Text(Cell(r, "Site")), 200),
                    AreaEquipment = Truncate(Text(Cell(r, "Area_Equipment")), 200),
                    Activity = Truncate(Text(Cell(r, "Activity")), 300),
                    Company = Truncate(Text(Cell(r, "Company")), 200),
                    Observer1 = Truncate(Text(Cell(r, "Observer_1")), 150),
                    Observer2 = Truncate(Text(Cell(r, "Observer_2")), 150),
                    Observer3 = Truncate(Text(Cell(r, "Observer_3")), 150),
                    YesCount = IntOrZero(Cell(r, "YES_Count")),
                    NoCount = IntOrZero(Cell(r, "NO_Count")),
                    NaCount = IntOrZero(Cell(r, "NA_Count")),
                    PerformancePercent = Percent(Cell(r, "Performance_%")),
                    ObservationSequence = Int(Cell(r, "Observation_Sequence")),
                    PsieEligible = YesNo(Cell(r, "PSIE_Eligible")),
                    Status = Truncate(Text(Cell(r, "Observation_Status")), 50),
                    IsActive = YesNo(Cell(r, "Active_Observation")),
                    ImportBatchId = batchId
                });
            }

            _context.Observations.AddRange(entities);
            // Disimpan lebih dulu agar Id-nya bisa dipakai menautkan seluruh tabel anak.
            await _context.SaveChangesAsync(cancellationToken);

            return entities.Count;
        }

        // ---------- Sheet INPUT-* ----------

        private int ImportSifQuestions(WorkbookAccessor wb, int batchId, IReadOnlyDictionary<string, int> obsIds, ICollection<string> warnings)
        {
            var entities = new List<SifQuestion>();

            foreach (var r in wb.ObjectRows("INPUT-SIF_Questions"))
            {
                var obsCode = Text(Cell(r, "Obs_ID"));
                if (!TryResolve(obsCode, obsIds, "INPUT-SIF_Questions", warnings, out var observationId))
                {
                    continue;
                }

                entities.Add(new SifQuestion
                {
                    ObservationId = observationId,
                    ProtocolCode = Truncate(Text(Cell(r, "Protocols_code")), 50),
                    ProtocolName = Truncate(Text(Cell(r, "Protocols_name")), 200),
                    QuestionRef = Truncate(Text(Cell(r, "Question_Ref")), 20),
                    CcvcId = Truncate(Text(Cell(r, "CCVC_ID")), 50),
                    QuestionText = Truncate(Text(Cell(r, "Observation_Question")), 1000),
                    Answer = ResolveAnswer(r),
                    Comments = Text(Cell(r, "Comments")),
                    SifExposure = Truncate(Text(Cell(r, "SIF_Exposure")), 200),
                    CriticalSafeguard = Truncate(Text(Cell(r, "Critical_Safeguard")), 200),
                    ObservationDate = Date(Cell(r, "Observation_Date")),
                    Zona = Int(Cell(r, "Zona")),
                    Site = Truncate(Text(Cell(r, "Site")), 200),
                    Activity = Truncate(Text(Cell(r, "Activity")), 300),
                    Company = Truncate(Text(Cell(r, "Company")), 200),
                    ImportBatchId = batchId
                });
            }

            _context.SifQuestions.AddRange(entities);
            return entities.Count;
        }

        /// <summary>Kolom YES/NO/NA berupa penanda; yang terisi lebih dulu menentukan jawaban.</summary>
        private static string ResolveAnswer(IReadOnlyDictionary<string, IXLCell> row)
        {
            if (IsMarked(Cell(row, "YES"))) return "YES";
            if (IsMarked(Cell(row, "NO"))) return "NO";
            if (IsMarked(Cell(row, "NA"))) return "NA";
            return "-";
        }

        private int ImportErrorTraps(WorkbookAccessor wb, int batchId, IReadOnlyDictionary<string, int> obsIds, ICollection<string> warnings)
        {
            // Sheet ini punya header ganda ("Protocols" dua kali) sehingga dibaca per indeks kolom.
            var entities = new List<ErrorTrap>();

            foreach (var r in wb.GridRows("INPUT-Error_Traps").Skip(1))
            {
                var obsCode = Text(At(r, 0));
                if (!TryResolve(obsCode, obsIds, "INPUT-Error_Traps", warnings, out var observationId))
                {
                    continue;
                }

                entities.Add(new ErrorTrap
                {
                    ObservationId = observationId,
                    ProtocolCode = Truncate(Text(At(r, 1)), 50),
                    ProtocolName = Truncate(Text(At(r, 2)), 200),
                    Category = Truncate(Text(At(r, 3)), 100),
                    TrapName = Truncate(Text(At(r, 4)), 200),
                    Comments = Text(At(r, 5)),
                    ImportBatchId = batchId
                });
            }

            _context.ErrorTraps.AddRange(entities);
            return entities.Count;
        }

        private int ImportHpTools(WorkbookAccessor wb, int batchId, IReadOnlyDictionary<string, int> obsIds, ICollection<string> warnings)
        {
            var entities = new List<HpTool>();

            foreach (var r in wb.GridRows("INPUT-HP_Tools").Skip(1))
            {
                var obsCode = Text(At(r, 0));
                if (!TryResolve(obsCode, obsIds, "INPUT-HP_Tools", warnings, out var observationId))
                {
                    continue;
                }

                entities.Add(new HpTool
                {
                    ObservationId = observationId,
                    ProtocolCode = Truncate(Text(At(r, 1)), 50),
                    ProtocolName = Truncate(Text(At(r, 2)), 200),
                    ToolName = Truncate(Text(At(r, 3)), 200),
                    Tujuan = Text(At(r, 4)),
                    KapanDigunakan = Text(At(r, 5)),
                    CaraPakai = Text(At(r, 6)),
                    EffectivenessNotes = Text(At(r, 7)),
                    ImportBatchId = batchId
                });
            }

            _context.HpTools.AddRange(entities);
            return entities.Count;
        }

        private int ImportDriftConditions(WorkbookAccessor wb, int batchId, IReadOnlyDictionary<string, int> obsIds, ICollection<string> warnings)
        {
            var entities = new List<DriftCondition>();

            foreach (var r in wb.GridRows("INPUT-Drift_Conditions").Skip(1))
            {
                var obsCode = Text(At(r, 0));
                if (!TryResolve(obsCode, obsIds, "INPUT-Drift_Conditions", warnings, out var observationId))
                {
                    continue;
                }

                entities.Add(new DriftCondition
                {
                    ObservationId = observationId,
                    ProtocolCode = Truncate(Text(At(r, 1)), 50),
                    ProtocolName = Truncate(Text(At(r, 2)), 200),
                    Situation = Text(At(r, 3)),
                    Level1 = Truncate(Text(At(r, 4)), 200),
                    Code = Truncate(Text(At(r, 5)), 50),
                    Level2 = Truncate(Text(At(r, 6)), 200),
                    Reason = Text(At(r, 7)),
                    Sequence = Int(At(r, 8)),
                    Status = Truncate(Text(At(r, 9)), 50),
                    IsActive = YesNo(At(r, 10)),
                    ImportBatchId = batchId
                });
            }

            _context.DriftConditions.AddRange(entities);
            return entities.Count;
        }

        private int ImportLatentConditions(WorkbookAccessor wb, int batchId, IReadOnlyDictionary<string, int> obsIds, ICollection<string> warnings)
        {
            var entities = new List<LatentCondition>();

            // Kolom sequence/status/active ada di indeks 10-12 (bukan 8-10 seperti sheet drift).
            foreach (var r in wb.GridRows("INPUT-Latent_Conditions").Skip(1))
            {
                var obsCode = Text(At(r, 0));
                if (!TryResolve(obsCode, obsIds, "INPUT-Latent_Conditions", warnings, out var observationId))
                {
                    continue;
                }

                entities.Add(new LatentCondition
                {
                    ObservationId = observationId,
                    ProtocolCode = Truncate(Text(At(r, 1)), 50),
                    ProtocolName = Truncate(Text(At(r, 2)), 200),
                    ObservationText = Text(At(r, 3)),
                    Level1 = Truncate(Text(At(r, 4)), 200),
                    Code = Truncate(Text(At(r, 5)), 50),
                    Level2 = Truncate(Text(At(r, 6)), 200),
                    Reason = Text(At(r, 7)),
                    Sequence = Int(At(r, 10)),
                    Status = Truncate(Text(At(r, 11)), 50),
                    IsActive = YesNo(At(r, 12)),
                    ImportBatchId = batchId
                });
            }

            _context.LatentConditions.AddRange(entities);
            return entities.Count;
        }

        // ---------- Sheet DATABASE & ANALYZE ----------

        private int ImportCcvcLibrary(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<CcvcLibraryItem>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var r in wb.ObjectRows("DATABASE_PSEC_CCVC"))
            {
                var ccvcId = Text(Cell(r, "CCVC_ID"));
                // CcvcId unik di database; duplikat di workbook diambil kemunculan pertama.
                if (string.IsNullOrEmpty(ccvcId) || !seen.Add(ccvcId))
                {
                    continue;
                }

                entities.Add(new CcvcLibraryItem
                {
                    RowNo = Int(Cell(r, "No")),
                    ProtocolGroup = Truncate(Text(Cell(r, "Protocol_Group")), 150),
                    PsecId = Truncate(Text(Cell(r, "PSEC_ID")), 50),
                    PsecName = Truncate(Text(Cell(r, "PSEC_Name")), 200),
                    ExposureType = Truncate(Text(Cell(r, "Exposure_Type")), 150),
                    CcvcId = Truncate(ccvcId, 50)!,
                    QuestionCode = Truncate(Text(Cell(r, "Question_Code")), 20),
                    QuestionSummary = Truncate(Text(Cell(r, "Question_Summary")), 500),
                    VerificationPurpose = Text(Cell(r, "Verification_Purpose")),
                    ImportBatchId = batchId
                });
            }

            _context.CcvcLibraryItems.AddRange(entities);
            return entities.Count;
        }

        private int ImportInitiatives(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<ImprovementInitiative>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var r in wb.ObjectRows("ANALYZE-IMPROVEMENT_INITIATIVES"))
            {
                var code = Text(Cell(r, "Improvement_ID"));
                if (string.IsNullOrEmpty(code) || !seen.Add(code))
                {
                    continue;
                }

                entities.Add(new ImprovementInitiative
                {
                    ImprovementCode = Truncate(code, 50)!,
                    Initiative = Truncate(Text(Cell(r, "Initiative")), 300),
                    RelatedClsr = Truncate(Text(Cell(r, "Related_CLSR")), 200),
                    Owner = Truncate(Text(Cell(r, "V&V_Team_Asset_Owner")), 150),
                    Status = Truncate(Text(Cell(r, "Status")), 50),
                    ProgressPercent = (int)(Percent(Cell(r, "Progress_%"), 0) ?? 0m),
                    ExpectedImpact = Text(Cell(r, "Expected_Impact")),
                    Notes = Text(Cell(r, "Notes")),
                    ImportBatchId = batchId
                });
            }

            _context.ImprovementInitiatives.AddRange(entities);
            return entities.Count;
        }

        private int ImportExecutiveMeasures(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<ExecutiveMeasure>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in wb.ObjectRows("ANALYZE-EXECUTIVE_MEASURES"))
            {
                var code = Text(Cell(r, "Metric_Code"));
                if (string.IsNullOrEmpty(code) || !seen.Add(code))
                {
                    continue;
                }

                entities.Add(new ExecutiveMeasure
                {
                    MetricCode = Truncate(code, 20)!,
                    MetricName = Truncate(Text(Cell(r, "Metric_Name")), 200),
                    Numerator = Decimal(Cell(r, "Numerator")),
                    Denominator = Decimal(Cell(r, "Denominator")),
                    ScorePercent = Percent(Cell(r, "Score_%")),
                    TargetPercent = Percent(Cell(r, "Target_%"), 0),
                    Status = Truncate(Text(Cell(r, "Status")), 50),
                    Notes = Text(Cell(r, "Notes")),
                    ImportBatchId = batchId
                });
            }

            _context.ExecutiveMeasures.AddRange(entities);
            return entities.Count;
        }

        private int ImportQuickFacts(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<QuickFact>();
            var order = 0;

            foreach (var r in wb.ObjectRows("ANALYZE-QUICK_FACTS"))
            {
                var name = Text(Cell(r, "Fact_Name"));
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var valueCell = Cell(r, "Fact_Value");
                var number = Number(valueCell);

                // Angka <= 1 di sheet ini selalu berupa rasio, jadi ditampilkan sebagai persen.
                // InvariantCulture wajib: nilai ini disimpan sebagai teks siap tampil dan
                // dibaca klien sebagai angka desimal bertitik, bukan mengikuti locale server.
                var display = number.HasValue && number.Value <= 1d
                    ? FormattableString.Invariant($"{Percent(valueCell)}%")
                    : Text(valueCell) ?? string.Empty;

                entities.Add(new QuickFact
                {
                    FactName = Truncate(name, 200)!,
                    FactValue = Truncate(display, 200),
                    Icon = QuickFactIcons.GetValueOrDefault(name, "clipboard"),
                    DisplayOrder = order++,
                    ImportBatchId = batchId
                });
            }

            _context.QuickFacts.AddRange(entities);
            return entities.Count;
        }

        private int ImportHealthMap(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<ClsrHealthMapRow>();
            var order = 0;

            foreach (var r in wb.ObjectRows("ANALYZE-CLSR_HEALTH_MAP"))
            {
                var clsrId = Text(Cell(r, "CLSR_ID"));
                if (string.IsNullOrEmpty(clsrId))
                {
                    continue;
                }

                entities.Add(new ClsrHealthMapRow
                {
                    ClsrId = Truncate(clsrId, 50)!,
                    ClsrDescription = Truncate(Text(Cell(r, "CLSR_Description")), 300),
                    Zona11Status = Truncate(Text(Cell(r, "Zona_11_Status")), 50),
                    Zona11Score = Percent(Cell(r, "Zona_11_Score")),
                    Zona12Status = Truncate(Text(Cell(r, "Zona_12_Status")), 50),
                    Zona12Score = Percent(Cell(r, "Zona_12_Score")),
                    Zona13Status = Truncate(Text(Cell(r, "Zona_13_Status")), 50),
                    Zona13Score = Percent(Cell(r, "Zona_13_Score")),
                    Zona14Status = Truncate(Text(Cell(r, "Zona_14_Status")), 50),
                    Zona14Score = Percent(Cell(r, "Zona_14_Score")),
                    Regional4Score = Percent(Cell(r, "Regional_4_Score")),
                    HealthStatus = Truncate(Text(Cell(r, "Health_Status")), 50),
                    DisplayOrder = order++,
                    ImportBatchId = batchId
                });
            }

            _context.ClsrHealthMapRows.AddRange(entities);
            return entities.Count;
        }

        private int ImportTopFive(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<TopFiveItem>();
            var order = 0;

            foreach (var r in wb.ObjectRows("ANALYZE-TOP5"))
            {
                var category = Text(Cell(r, "Category"));
                if (string.IsNullOrEmpty(category))
                {
                    continue;
                }

                entities.Add(new TopFiveItem
                {
                    Category = Truncate(category, 100)!,
                    Item = Truncate(Text(Cell(r, "Item")), 300),
                    Count = IntOrZero(Cell(r, "Count")),
                    // Disimpan sebagai rasio 0-1 seperti di Excel; konversi ke persen
                    // dilakukan DashboardService saat menyusun label.
                    Percent = Decimal(Cell(r, "Percent")),
                    Denominator = Int(Cell(r, "Denominator")),
                    DisplayOrder = order++,
                    ImportBatchId = batchId
                });
            }

            _context.TopFiveItems.AddRange(entities);
            return entities.Count;
        }

        private int ImportDashboardTexts(WorkbookAccessor wb, int batchId)
        {
            var entities = new List<DashboardText>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in wb.ObjectRows("CONFIG-DASHBOARD_TEXT"))
            {
                var section = Text(Cell(r, "Section"));
                if (string.IsNullOrEmpty(section) || !seen.Add(section))
                {
                    continue;
                }

                entities.Add(new DashboardText
                {
                    Section = Truncate(section, 150)!,
                    Text = Text(Cell(r, "Text")),
                    ImportBatchId = batchId
                });
            }

            _context.DashboardTexts.AddRange(entities);
            return entities.Count;
        }

        /// <summary>
        /// Sheet ANALYZE-TREND_ZONE memuat dua tabel berdampingan: tren bulanan di kolom
        /// kiri dan skor per zona di kolom kanan. Keduanya dibaca dalam satu lintasan.
        /// </summary>
        private (int TrendCount, int ZonaCount) ImportTrendAndZona(WorkbookAccessor wb, int batchId)
        {
            var trendPoints = new List<TrendPoint>();
            var zonaScores = new List<ZonaScore>();
            var trendOrder = 0;
            var zonaOrder = 0;
            var hasActualYet = false;
            var seenMonths = new HashSet<DateOnly>();
            var seenZonas = new HashSet<int>();

            foreach (var r in wb.GridRows("ANALYZE-TREND_ZONE").Skip(1))
            {
                var month = Date(At(r, 0));
                if (month.HasValue && seenMonths.Add(month.Value))
                {
                    var actual = Percent(At(r, 4));
                    var plannedText = Text(At(r, 5));
                    var planned = string.Equals(plannedText, "N/A", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : Percent(At(r, 5));
                    var obsCount = IntOrZero(At(r, 6));

                    // Bulan dengan observasi = realisasi. Bulan tanpa realisasi tetapi
                    // punya angka rencana = proyeksi, dan hanya dihitung setelah
                    // setidaknya satu bulan realisasi ada (mengikuti converter).
                    if (actual.HasValue && obsCount > 0)
                    {
                        hasActualYet = true;
                        trendPoints.Add(NewTrendPoint(month.Value, actual, planned, obsCount, false, trendOrder++, batchId));
                    }
                    else if (!actual.HasValue && planned.HasValue && hasActualYet)
                    {
                        trendPoints.Add(NewTrendPoint(month.Value, null, planned, obsCount, true, trendOrder++, batchId));
                    }
                }

                var zona = Int(At(r, 8));
                if (zona.HasValue && seenZonas.Add(zona.Value))
                {
                    zonaScores.Add(new ZonaScore
                    {
                        Zona = zona.Value,
                        ZonaLabel = $"Zona {zona.Value}",
                        ScorePercent = Percent(At(r, 12)) ?? 0m,
                        ObservationCount = IntOrZero(At(r, 13)),
                        DisplayOrder = zonaOrder++,
                        ImportBatchId = batchId
                    });
                }
            }

            _context.TrendPoints.AddRange(trendPoints);
            _context.ZonaScores.AddRange(zonaScores);

            return (trendPoints.Count, zonaScores.Count);
        }

        private static TrendPoint NewTrendPoint(
            DateOnly month, decimal? actual, decimal? planned, int obsCount, bool isProjection, int order, int batchId)
            => new()
            {
                PeriodMonth = month,
                MonthLabel = $"{MonthNames[month.Month - 1]}-{month.Year % 100:D2}",
                ActualPercent = actual,
                PlannedPercent = planned,
                ObservationCount = obsCount,
                IsProjection = isProjection,
                DisplayOrder = order,
                ImportBatchId = batchId
            };

        // ---------- Worksheet mentah (untuk viewer generik & menu sidebar) ----------

        /// <summary>
        /// Hanya sheet yang dipakai aplikasi yang disimpan. Sheet bantu internal Excel
        /// (Helper_*, AUDIT-*, ReadMe, dsb.) sengaja dilewati, sama seperti converter.
        /// </summary>
        private async Task<int> ImportWorksheetsAsync(WorkbookAccessor wb, int batchId, CancellationToken cancellationToken)
        {
            var usedSlugs = new HashSet<string>(StringComparer.Ordinal);
            var index = 0;
            var saved = 0;

            foreach (var name in wb.SheetNames.Where(SheetSchema.RequiredSheetNames.Contains))
            {
                var slug = SheetSchema.Slugify(name);
                while (!usedSlugs.Add(slug))
                {
                    slug += "-x";
                }

                var (rows, colCount) = wb.FormattedRows(name);
                var curated = SheetSchema.Curated.GetValueOrDefault(name);
                var group = SheetSchema.GroupOf(name);

                var worksheet = new Worksheet
                {
                    Name = Truncate(name, 200)!,
                    Slug = Truncate(slug, 200)!,
                    SheetIndex = index++,
                    GroupName = group,
                    Label = Truncate(curated?.Label ?? SheetSchema.ShortLabel(name), 200),
                    Icon = curated?.Icon ?? SheetSchema.IconForGroup(group),
                    Route = curated?.Route ?? $"/sheet/{slug}",
                    IsCurated = curated is not null,
                    IsRequired = true,
                    RowCount = rows.Count,
                    ColCount = colCount,
                    ImportBatchId = batchId
                };

                _context.Worksheets.Add(worksheet);
                await _context.SaveChangesAsync(cancellationToken);

                var rowIndex = 0;
                var entities = rows.Select(row => new WorksheetRow
                {
                    WorksheetId = worksheet.Id,
                    ExcelRow = row.ExcelRow,
                    RowIndex = rowIndex++,
                    CellsJson = JsonSerializer.Serialize(row.Cells)
                }).ToList();

                _context.WorksheetRows.AddRange(entities);
                await _context.SaveChangesAsync(cancellationToken);

                // Baris disimpan per sheet lalu dilepas dari change tracker supaya
                // memori tidak menumpuk pada workbook dengan puluhan ribu baris.
                _context.ChangeTracker.Clear();
                saved++;
            }

            return saved;
        }

        // ---------- Utilitas ----------

        private static IXLCell? Cell(IReadOnlyDictionary<string, IXLCell> row, string column)
            => row.GetValueOrDefault(column);

        private static IXLCell? At(IXLCell[] row, int index)
            => index >= 0 && index < row.Length ? row[index] : null;

        /// <summary>
        /// Menautkan baris anak ke observasi induknya. Baris yang menunjuk Obs_ID
        /// tidak dikenal dilewati dengan peringatan, bukan menggagalkan seluruh import.
        /// </summary>
        private static bool TryResolve(
            string? obsCode,
            IReadOnlyDictionary<string, int> obsIds,
            string sheetName,
            ICollection<string> warnings,
            out int observationId)
        {
            observationId = 0;

            if (string.IsNullOrEmpty(obsCode))
            {
                return false;
            }

            if (!obsIds.TryGetValue(obsCode, out observationId))
            {
                var message = $"{sheetName}: Obs_ID \"{obsCode}\" tidak ada di ANALYZE-CONFORMANCE_SCORE, baris dilewati.";
                if (!warnings.Contains(message))
                {
                    warnings.Add(message);
                }

                return false;
            }

            return true;
        }

        public Task<PagedResult<ImportBatchDto>> GetBatchesAsync(QueryParameters query, CancellationToken cancellationToken = default)
            => _context.ImportBatches.AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToPagedResultAsync(query, x => x.ToDto(), cancellationToken);

        public async Task<ImportBatchDto?> GetBatchAsync(int id, CancellationToken cancellationToken = default)
        {
            var batch = await _context.ImportBatches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return batch?.ToDto();
        }
    }
}
