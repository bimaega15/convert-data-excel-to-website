namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Satu baris mentah worksheet, dipakai viewer generik di Vue.
    /// Sel disimpan sebagai array JSON (bukan kolom terpisah) karena jumlah kolom
    /// berbeda-beda antar sheet dan skema tidak boleh berubah saat workbook berubah.
    /// </summary>
    public class WorksheetRow
    {
        public long Id { get; set; }

        public int WorksheetId { get; set; }
        public Worksheet? Worksheet { get; set; }

        /// <summary>Nomor baris asli di Excel (1-based), supaya referensi sel tetap cocok.</summary>
        public int ExcelRow { get; set; }

        /// <summary>Urutan tampil (0 = baris header).</summary>
        public int RowIndex { get; set; }

        /// <summary>Isi sel sebagai JSON array of string, mis. ["OBS-001","WAH",...].</summary>
        public string CellsJson { get; set; } = "[]";
    }
}
