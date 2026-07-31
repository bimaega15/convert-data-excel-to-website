namespace Sifp_Vue.Server.Models.Dtos
{
    /// <summary>
    /// Padanan <c>sheets/_manifest.json</c>. Menjadi sumber tunggal menu sidebar Vue,
    /// jadi jumlah menu otomatis mengikuti isi workbook terakhir yang diimport.
    /// </summary>
    public class WorksheetManifestDto
    {
        public DateTime GeneratedAt { get; set; }
        public string? SourceFile { get; set; }
        public int SheetCount { get; set; }
        public List<WorksheetGroupDto> Groups { get; set; } = new();
    }

    public class WorksheetGroupDto
    {
        public string Label { get; set; } = string.Empty;
        public List<WorksheetItemDto> Items { get; set; } = new();
    }

    public class WorksheetItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Index { get; set; }
        public string? Group { get; set; }
        public string? Label { get; set; }
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public bool Curated { get; set; }
        public int RowCount { get; set; }
        public int ColCount { get; set; }

        /// <summary>Jumlah baris data (tanpa baris header).</summary>
        public int DataRows { get; set; }
    }

    /// <summary>Padanan <c>sheets/&lt;slug&gt;.json</c> untuk viewer generik.</summary>
    public class WorksheetDataDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public int ColCount { get; set; }
        public List<string[]> Rows { get; set; } = new();
    }
}
