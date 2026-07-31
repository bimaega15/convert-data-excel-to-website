using System.Globalization;
using ClosedXML.Excel;

namespace Sifp_Vue.Server.Helpers
{
    /// <summary>
    /// Pembacaan sel Excel yang toleran terhadap variasi tipe: satu kolom bisa berisi
    /// angka asli di satu baris dan teks di baris lain. Semua konversi dipusatkan di
    /// sini supaya aturannya sama untuk seluruh sheet.
    /// </summary>
    public static class ExcelCellReader
    {
        public static string? Text(IXLCell? cell)
        {
            if (cell is null || cell.IsEmpty())
            {
                return null;
            }

            var value = cell.Value;
            if (value.IsBlank)
            {
                return null;
            }

            // GetFormattedString() dipakai untuk non-teks supaya tanggal dan persen
            // tampil seperti di Excel, bukan sebagai angka serial.
            var text = (value.IsText ? value.GetText() : cell.GetFormattedString())?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        /// <summary>Nilai sel apa adanya untuk disimpan mentah (tidak pernah null).</summary>
        public static string FormattedOrEmpty(IXLCell? cell)
        {
            if (cell is null || cell.IsEmpty())
            {
                return string.Empty;
            }

            return cell.GetFormattedString()?.Trim() ?? string.Empty;
        }

        public static double? Number(IXLCell? cell)
        {
            if (cell is null || cell.IsEmpty())
            {
                return null;
            }

            var value = cell.Value;
            if (value.IsNumber)
            {
                return value.GetNumber();
            }

            if (value.IsText)
            {
                var raw = value.GetText().Trim().Replace("%", string.Empty);
                if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        public static int? Int(IXLCell? cell)
        {
            var number = Number(cell);
            return number.HasValue ? (int)Math.Round(number.Value, MidpointRounding.AwayFromZero) : null;
        }

        public static int IntOrZero(IXLCell? cell) => Int(cell) ?? 0;

        public static decimal? Decimal(IXLCell? cell)
        {
            var number = Number(cell);
            return number.HasValue ? (decimal)number.Value : null;
        }

        /// <summary>
        /// Excel menyimpan persen sebagai pecahan (0.4444). Nilai dikembalikan dalam
        /// skala 0-100 agar sama dengan hasil <c>pct()</c> di converter JavaScript.
        /// Nilai yang sudah &gt; 1 dianggap sudah dalam persen dan tidak dikali lagi.
        /// </summary>
        public static decimal? Percent(IXLCell? cell, int digits = 2)
        {
            var number = Number(cell);
            if (!number.HasValue)
            {
                return null;
            }

            var scaled = Math.Abs(number.Value) <= 1d ? number.Value * 100d : number.Value;
            return Math.Round((decimal)scaled, digits, MidpointRounding.AwayFromZero);
        }

        public static DateOnly? Date(IXLCell? cell)
        {
            if (cell is null || cell.IsEmpty())
            {
                return null;
            }

            var value = cell.Value;

            if (value.IsDateTime)
            {
                return DateOnly.FromDateTime(value.GetDateTime());
            }

            // Serial Excel: dikonversi lewat OADate, bukan aritmetika manual,
            // supaya bug tahun 1900 milik Excel tetap ditangani dengan benar.
            if (value.IsNumber)
            {
                var serial = value.GetNumber();
                if (serial > 0 && serial < 2958466)
                {
                    return DateOnly.FromDateTime(DateTime.FromOADate(serial));
                }
            }

            if (value.IsText && DateTime.TryParse(value.GetText(), CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed))
            {
                return DateOnly.FromDateTime(parsed);
            }

            return null;
        }

        /// <summary>Kolom "Y"/"N" di workbook. Nilai kosong dianggap false.</summary>
        public static bool YesNo(IXLCell? cell)
        {
            var text = Text(cell);
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text.Equals("Y", StringComparison.OrdinalIgnoreCase)
                   || text.Equals("YES", StringComparison.OrdinalIgnoreCase)
                   || text.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
                   || text == "1";
        }

        /// <summary>True bila sel berisi tanda centang/isian, dipakai kolom YES / NO / NA.</summary>
        public static bool IsMarked(IXLCell? cell) => !string.IsNullOrWhiteSpace(Text(cell));

        /// <summary>Memotong teks agar muat pada kolom dengan panjang terbatas.</summary>
        public static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }
    }
}
