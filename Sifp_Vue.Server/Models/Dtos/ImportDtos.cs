namespace Sifp_Vue.Server.Models.Dtos
{
    /// <summary>
    /// Satu perubahan sel dari layar preview. Bentuknya mengikuti
    /// <c>flattenEdits()</c> di <c>sifp_vue.client/src/services/excelImport.js</c>:
    /// penomoran baris/kolom memakai penomoran Excel (1-based).
    /// </summary>
    public class CellEditDto
    {
        public string Sheet { get; set; } = string.Empty;
        public int ExcelRow { get; set; }
        public int ExcelCol { get; set; }

        /// <summary>Referensi sel gaya Excel, mis. "C7".</summary>
        public string? Cell { get; set; }

        public string? From { get; set; }
        public string? To { get; set; }
    }

    /// <summary>Ringkasan hasil parse yang dikirim klien bersama file (buildSummary()).</summary>
    public class ImportSummaryDto
    {
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public int SheetCount { get; set; }
        public int TotalRows { get; set; }
        public List<ImportSummarySheetDto> Sheets { get; set; } = new();
        public int EditCount { get; set; }
    }

    public class ImportSummarySheetDto
    {
        public string Name { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Cols { get; set; }
        public bool Required { get; set; }
    }

    /// <summary>Hasil import yang dikembalikan ke halaman Import Excel di Vue.</summary>
    public class ImportResultDto
    {
        public int BatchId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int SheetCount { get; set; }
        public int EditsApplied { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>Jumlah baris yang tersimpan per tabel master, mis. {"Observations": 23}.</summary>
        public Dictionary<string, int> RowsImported { get; set; } = new();

        /// <summary>Peringatan non-fatal, mis. sheet opsional yang dilewati.</summary>
        public List<string> Warnings { get; set; } = new();
    }

    public class ImportBatchDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SheetCount { get; set; }
        public int TotalRows { get; set; }
        public int EditCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
