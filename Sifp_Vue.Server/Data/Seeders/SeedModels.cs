using System.Text.Json.Serialization;

namespace Sifp_Vue.Server.Data.Seeders
{
    // Bentuk file JSON hasil `npm run convert:excel` di sifp_vue.client/src/data/generated.
    // Hanya dipakai seeder; API memakai DTO di Models/Dtos.

    public class SeedObservation
    {
        public string? Id { get; set; }
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Date { get; set; }
        public int? Zona { get; set; }
        public string? Site { get; set; }
        public string? Area { get; set; }
        public string? Activity { get; set; }
        public string? Company { get; set; }
        public List<string>? Observers { get; set; }
        public int Yes { get; set; }
        public int No { get; set; }
        public int Na { get; set; }
        public decimal? Performance { get; set; }
        public int? Sequence { get; set; }
        public string? PsieEligible { get; set; }
        public string? Status { get; set; }
        public string? Active { get; set; }
    }

    public class SeedSifQuestion
    {
        public string? ObsId { get; set; }
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? QuestionRef { get; set; }
        public string? CcvcId { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public string? Comments { get; set; }
        public string? SifExposure { get; set; }
        public string? CriticalSafeguard { get; set; }
        public string? Date { get; set; }
        public int? Zona { get; set; }
        public string? Site { get; set; }
        public string? Activity { get; set; }
        public string? Company { get; set; }
    }

    public class SeedCcvcItem
    {
        public int? No { get; set; }
        public string? ProtocolGroup { get; set; }
        public string? PsecId { get; set; }
        public string? PsecName { get; set; }
        public string? ExposureType { get; set; }
        public string? CcvcId { get; set; }
        public string? QuestionCode { get; set; }
        public string? QuestionSummary { get; set; }
        public string? VerificationPurpose { get; set; }
    }

    public class SeedErrorTrap
    {
        public string? ObsId { get; set; }
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Category { get; set; }
        public string? ErrorTrap { get; set; }
        public string? Comments { get; set; }
    }

    public class SeedHpTool
    {
        public string? ObsId { get; set; }
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Tool { get; set; }
        public string? Tujuan { get; set; }
        public string? KapanDigunakan { get; set; }
        public string? CaraPakai { get; set; }
        public string? EffectivenessNotes { get; set; }
    }

    public class SeedDriftCondition
    {
        public string? ObsId { get; set; }
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Situation { get; set; }
        public string? Level1 { get; set; }
        public string? Code { get; set; }
        public string? Level2 { get; set; }
        public string? Reason { get; set; }
        public int? Sequence { get; set; }
        public string? Status { get; set; }
        public string? Active { get; set; }
    }

    public class SeedLatentCondition
    {
        public string? ObsId { get; set; }
        public string? ProtocolCode { get; set; }
        public string? ProtocolName { get; set; }
        public string? Observation { get; set; }
        public string? Level1 { get; set; }
        public string? Code { get; set; }
        public string? Level2 { get; set; }
        public string? Reason { get; set; }
        public int? Sequence { get; set; }
        public string? Status { get; set; }
        public string? Active { get; set; }
    }

    public class SeedInitiative
    {
        public string? Id { get; set; }
        public string? Initiative { get; set; }
        public string? RelatedClsr { get; set; }
        public string? Owner { get; set; }
        public string? Status { get; set; }
        public int? Progress { get; set; }
        public string? ExpectedImpact { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- dashboard.json ----------

    public class SeedDashboard
    {
        public SeedDashboardMeta? Meta { get; set; }
        public List<SeedKpi>? Kpis { get; set; }
        public SeedConformance? Conformance { get; set; }
        public List<SeedQuickFact>? QuickFacts { get; set; }
        public SeedHealthMap? HealthMap { get; set; }
        public List<SeedTopPanel>? TopPanels { get; set; }
        public SeedTrend? Trend { get; set; }
        public SeedZonaScores? ZonaScores { get; set; }
        public List<SeedSummaryCard>? SummaryCards { get; set; }
        public List<string>? SummaryNotes { get; set; }
    }

    public class SeedDashboardMeta
    {
        public string? SourceFile { get; set; }
        public DateTime? GeneratedAt { get; set; }
    }

    public class SeedKpi
    {
        public string? Code { get; set; }
        public string? Title { get; set; }
        public decimal? Value { get; set; }
        public bool Pending { get; set; }
        public string? Desc { get; set; }
        public string? Target { get; set; }
    }

    public class SeedConformance
    {
        public decimal? Value { get; set; }
        public string? Target { get; set; }
    }

    public class SeedQuickFact
    {
        public string? Icon { get; set; }
        public string? Label { get; set; }
        public string? Value { get; set; }
    }

    public class SeedHealthMap
    {
        public List<SeedHealthRow>? Rows { get; set; }
    }

    public class SeedHealthRow
    {
        public string? Name { get; set; }
        public List<SeedHealthCell>? Cells { get; set; }
        public decimal? Regional { get; set; }
        public string? RegionalStatus { get; set; }
    }

    public class SeedHealthCell
    {
        public string? Status { get; set; }
        public decimal? Score { get; set; }
    }

    public class SeedTopPanel
    {
        public int No { get; set; }
        public List<SeedTopItem>? Items { get; set; }
        public SeedTopFooter? Footer { get; set; }
    }

    public class SeedTopItem
    {
        public string? Label { get; set; }

        /// <summary>Berformat "12" atau "12 (52%)".</summary>
        public string? Display { get; set; }
    }

    public class SeedTopFooter
    {
        public int Value { get; set; }
    }

    public class SeedTrend
    {
        public decimal? Target { get; set; }
        public List<SeedTrendPoint>? Points { get; set; }
        public List<SeedTrendPoint>? Projection { get; set; }
    }

    public class SeedTrendPoint
    {
        /// <summary>Berformat "May-26".</summary>
        public string? Month { get; set; }

        public decimal Value { get; set; }
    }

    public class SeedZonaScores
    {
        public List<SeedZonaBar>? Bars { get; set; }
    }

    public class SeedZonaBar
    {
        public string? Zone { get; set; }
        public int Obs { get; set; }
        public decimal Value { get; set; }
    }

    public class SeedSummaryCard
    {
        public string? Title { get; set; }
        public string? Text { get; set; }
    }

    // ---------- sheets/_manifest.json & sheets/<slug>.json ----------

    public class SeedManifest
    {
        public DateTime? GeneratedAt { get; set; }
        public string? SourceFile { get; set; }
        public int SheetCount { get; set; }
        public List<SeedManifestGroup>? Groups { get; set; }
    }

    public class SeedManifestGroup
    {
        public string? Label { get; set; }
        public List<SeedManifestItem>? Items { get; set; }
    }

    public class SeedManifestItem
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public int Index { get; set; }
        public string? Group { get; set; }
        public string? Label { get; set; }
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public bool Curated { get; set; }
        public int RowCount { get; set; }
        public int ColCount { get; set; }
    }

    public class SeedSheetData
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public int RowCount { get; set; }
        public int ColCount { get; set; }

        [JsonConverter(typeof(JsonStringArrayListConverter))]
        public List<string[]>? Rows { get; set; }
    }

    /// <summary>
    /// Kolom teks di workbook tidak selalu berisi teks — sel yang kosong di Excel
    /// bisa keluar sebagai angka (mis. <c>"company": 0</c>). Konverter ini menerima
    /// string, angka, maupun boolean lalu menormalkannya menjadi string.
    /// </summary>
    public class FlexibleStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
            => reader.TokenType switch
            {
                System.Text.Json.JsonTokenType.String => reader.GetString(),
                System.Text.Json.JsonTokenType.Null => null,
                System.Text.Json.JsonTokenType.Number => reader.TryGetInt64(out var i)
                    ? i.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                System.Text.Json.JsonTokenType.True => "true",
                System.Text.Json.JsonTokenType.False => "false",
                _ => throw new System.Text.Json.JsonException(
                    $"Tidak dapat membaca token {reader.TokenType} sebagai teks.")
            };

        public override void Write(System.Text.Json.Utf8JsonWriter writer, string? value, System.Text.Json.JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }

    /// <summary>
    /// Sel di file sheet bisa berupa string, angka, atau null. Konverter ini
    /// menormalkannya menjadi string supaya bentuk baris selalu seragam.
    /// </summary>
    public class JsonStringArrayListConverter : JsonConverter<List<string[]>>
    {
        public override List<string[]> Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            using var document = System.Text.Json.JsonDocument.ParseValue(ref reader);
            var rows = new List<string[]>();

            foreach (var rowElement in document.RootElement.EnumerateArray())
            {
                rows.Add(rowElement.EnumerateArray()
                    .Select(cell => cell.ValueKind switch
                    {
                        System.Text.Json.JsonValueKind.String => cell.GetString() ?? string.Empty,
                        System.Text.Json.JsonValueKind.Null or System.Text.Json.JsonValueKind.Undefined => string.Empty,
                        _ => cell.ToString()
                    })
                    .ToArray());
            }

            return rows;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, List<string[]> value, System.Text.Json.JsonSerializerOptions options)
            => System.Text.Json.JsonSerializer.Serialize(writer, value, options);
    }
}
