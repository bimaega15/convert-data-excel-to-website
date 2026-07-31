using ClosedXML.Excel;

namespace Sifp_Vue.Server.Helpers
{
    /// <summary>
    /// Pembungkus tipis di atas ClosedXML yang menyediakan dua cara baca yang dipakai
    /// converter JavaScript: berbasis nama header (<c>objRows</c>) dan berbasis indeks
    /// kolom (<c>gridRows</c>, untuk sheet yang punya header ganda/kosong).
    /// </summary>
    public sealed class WorkbookAccessor : IDisposable
    {
        private readonly XLWorkbook _workbook;

        public WorkbookAccessor(Stream stream)
        {
            _workbook = new XLWorkbook(stream);
        }

        public IReadOnlyList<string> SheetNames => _workbook.Worksheets.Select(w => w.Name).ToList();

        public bool HasSheet(string name) => _workbook.Worksheets.Any(w => w.Name == name);

        public IXLWorksheet? Sheet(string name) =>
            _workbook.Worksheets.FirstOrDefault(w => w.Name == name);

        /// <summary>
        /// Menerapkan perubahan sel dari layar preview ke workbook asli.
        /// Dilakukan sebelum parsing supaya seluruh tabel turunan ikut memakai nilai baru.
        /// </summary>
        public int ApplyEdits(IEnumerable<Models.Dtos.CellEditDto> edits, ICollection<string> warnings)
        {
            var applied = 0;

            foreach (var edit in edits)
            {
                var sheet = Sheet(edit.Sheet);
                if (sheet is null)
                {
                    warnings.Add($"Edit dilewati: sheet \"{edit.Sheet}\" tidak ada di workbook.");
                    continue;
                }

                if (edit.ExcelRow < 1 || edit.ExcelCol < 1)
                {
                    warnings.Add($"Edit dilewati: posisi sel tidak valid ({edit.Sheet}!{edit.Cell}).");
                    continue;
                }

                // Nilai selalu ditulis sebagai teks: preview di klien juga menampilkan
                // dan mengedit teks terformat, jadi menebak tipe di sini justru berisiko.
                sheet.Cell(edit.ExcelRow, edit.ExcelCol).Value = edit.To ?? string.Empty;
                applied++;
            }

            return applied;
        }

        /// <summary>
        /// Baris sheet sebagai peta nama-kolom → sel. Baris pertama dipakai sebagai header.
        /// Header duplikat memakai kemunculan pertama, sama seperti SheetJS.
        /// </summary>
        public List<Dictionary<string, IXLCell>> ObjectRows(string sheetName)
        {
            var result = new List<Dictionary<string, IXLCell>>();
            var sheet = Sheet(sheetName);
            var range = sheet?.RangeUsed();
            if (sheet is null || range is null)
            {
                return result;
            }

            var firstRow = range.FirstRow().RowNumber();
            var lastRow = range.LastRow().RowNumber();
            var firstCol = range.FirstColumn().ColumnNumber();
            var lastCol = range.LastColumn().ColumnNumber();

            var headers = new Dictionary<int, string>();
            for (var col = firstCol; col <= lastCol; col++)
            {
                var name = ExcelCellReader.Text(sheet.Cell(firstRow, col));
                if (!string.IsNullOrEmpty(name) && !headers.ContainsValue(name))
                {
                    headers[col] = name;
                }
            }

            for (var row = firstRow + 1; row <= lastRow; row++)
            {
                var map = new Dictionary<string, IXLCell>(StringComparer.Ordinal);
                foreach (var (col, name) in headers)
                {
                    map[name] = sheet.Cell(row, col);
                }

                result.Add(map);
            }

            return result;
        }

        /// <summary>Baris sheet sebagai larik sel per indeks kolom (0-based), termasuk baris header.</summary>
        public List<IXLCell[]> GridRows(string sheetName)
        {
            var result = new List<IXLCell[]>();
            var sheet = Sheet(sheetName);
            var range = sheet?.RangeUsed();
            if (sheet is null || range is null)
            {
                return result;
            }

            var firstRow = range.FirstRow().RowNumber();
            var lastRow = range.LastRow().RowNumber();
            var firstCol = range.FirstColumn().ColumnNumber();
            var lastCol = range.LastColumn().ColumnNumber();
            var width = lastCol - firstCol + 1;

            for (var row = firstRow; row <= lastRow; row++)
            {
                var cells = new IXLCell[width];
                for (var i = 0; i < width; i++)
                {
                    cells[i] = sheet.Cell(row, firstCol + i);
                }

                result.Add(cells);
            }

            return result;
        }

        /// <summary>
        /// Isi sheet apa adanya sebagai teks terformat, untuk disimpan ke WorksheetRows.
        /// Baris yang seluruh selnya kosong dibuang, tetapi nomor baris Excel aslinya
        /// tetap dicatat supaya referensi sel di layar preview tidak bergeser.
        /// </summary>
        public (List<(int ExcelRow, string[] Cells)> Rows, int ColCount) FormattedRows(string sheetName)
        {
            var rows = new List<(int, string[])>();
            var sheet = Sheet(sheetName);
            var range = sheet?.RangeUsed();
            if (sheet is null || range is null)
            {
                return (rows, 0);
            }

            var firstRow = range.FirstRow().RowNumber();
            var lastRow = range.LastRow().RowNumber();
            var firstCol = range.FirstColumn().ColumnNumber();
            var lastCol = range.LastColumn().ColumnNumber();
            var width = lastCol - firstCol + 1;

            for (var row = firstRow; row <= lastRow; row++)
            {
                var cells = new string[width];
                var hasContent = false;

                for (var i = 0; i < width; i++)
                {
                    cells[i] = ExcelCellReader.FormattedOrEmpty(sheet.Cell(row, firstCol + i));
                    if (cells[i].Length > 0)
                    {
                        hasContent = true;
                    }
                }

                if (hasContent)
                {
                    rows.Add((row, cells));
                }
            }

            return (rows, width);
        }

        public void Dispose() => _workbook.Dispose();
    }
}
