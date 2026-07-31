using System.Text.Json;
using Sifp_Vue.Server.Helpers;
using Sifp_Vue.Server.Models.Dtos;
using Sifp_Vue.Server.Repositories;
using Sifp_Vue.Server.Services.Contracts;

namespace Sifp_Vue.Server.Services
{
    public class WorksheetService : IWorksheetService
    {
        private readonly IWorksheetRepository _worksheets;
        private readonly ILogger<WorksheetService> _logger;

        public WorksheetService(IWorksheetRepository worksheets, ILogger<WorksheetService> logger)
        {
            _worksheets = worksheets;
            _logger = logger;
        }

        public async Task<WorksheetManifestDto> GetManifestAsync(CancellationToken cancellationToken = default)
        {
            var batch = await _worksheets.GetLatestCompletedBatchAsync(cancellationToken);
            if (batch is null)
            {
                // Belum ada import yang berhasil: kembalikan manifest kosong, bukan error,
                // supaya sidebar Vue tetap bisa dirender.
                return new WorksheetManifestDto { GeneratedAt = DateTime.UtcNow, SheetCount = 0 };
            }

            var sheets = await _worksheets.GetByBatchAsync(batch.Id, cancellationToken);

            var items = sheets.Select(s => new WorksheetItemDto
            {
                Name = s.Name,
                Slug = s.Slug,
                Index = s.SheetIndex,
                Group = s.GroupName,
                Label = s.Label,
                Icon = s.Icon,
                Route = s.Route,
                Curated = s.IsCurated,
                RowCount = s.RowCount,
                ColCount = s.ColCount,
                DataRows = Math.Max(0, s.RowCount - 1)
            }).ToList();

            // Grup mengikuti GroupOrder; grup kosong tidak ditampilkan.
            var groups = SheetSchema.GroupOrder
                .Select(label => new WorksheetGroupDto
                {
                    Label = label,
                    Items = items.Where(i => i.Group == label).ToList()
                })
                .Where(g => g.Items.Count > 0)
                .ToList();

            return new WorksheetManifestDto
            {
                GeneratedAt = batch.CompletedAt ?? batch.CreatedAt,
                SourceFile = batch.FileName,
                SheetCount = items.Count,
                Groups = groups
            };
        }

        public async Task<WorksheetDataDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var batch = await _worksheets.GetLatestCompletedBatchAsync(cancellationToken);
            if (batch is null)
            {
                return null;
            }

            var sheet = await _worksheets.GetBySlugAsync(batch.Id, slug, cancellationToken);
            if (sheet is null)
            {
                return null;
            }

            var rows = await _worksheets.GetRowsAsync(sheet.Id, cancellationToken);

            return new WorksheetDataDto
            {
                Name = sheet.Name,
                Slug = sheet.Slug,
                RowCount = sheet.RowCount,
                ColCount = sheet.ColCount,
                Rows = rows.Select(r => Deserialize(r.CellsJson, sheet.ColCount)).ToList()
            };
        }

        /// <summary>
        /// Baris disimpan sebagai JSON array. Panjangnya diseragamkan ke jumlah kolom
        /// sheet supaya grid di klien tidak bergerigi bila ada baris pendek.
        /// </summary>
        private string[] Deserialize(string cellsJson, int colCount)
        {
            try
            {
                var cells = JsonSerializer.Deserialize<string[]>(cellsJson) ?? Array.Empty<string>();
                if (cells.Length == colCount)
                {
                    return cells;
                }

                var padded = new string[colCount];
                for (var i = 0; i < colCount; i++)
                {
                    padded[i] = i < cells.Length ? cells[i] ?? string.Empty : string.Empty;
                }

                return padded;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "CellsJson tidak valid, baris dikembalikan kosong");
                return new string[colCount];
            }
        }
    }
}
