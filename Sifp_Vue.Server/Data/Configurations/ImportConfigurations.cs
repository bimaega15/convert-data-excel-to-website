using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Data.Configurations
{
    public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
    {
        public void Configure(EntityTypeBuilder<ImportBatch> b)
        {
            b.ToTable("ImportBatches");
            b.HasKey(x => x.Id);

            b.Property(x => x.FileName).HasMaxLength(400).IsRequired();
            b.Property(x => x.FileHash).HasMaxLength(64);
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.EditsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.SummaryJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.ErrorMessage).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.FileHash);
        }
    }

    public class WorksheetConfiguration : IEntityTypeConfiguration<Worksheet>
    {
        public void Configure(EntityTypeBuilder<Worksheet> b)
        {
            b.ToTable("Worksheets");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            b.Property(x => x.GroupName).HasMaxLength(100);
            b.Property(x => x.Label).HasMaxLength(200);
            b.Property(x => x.Icon).HasMaxLength(50);
            b.Property(x => x.Route).HasMaxLength(300);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            // Slug hanya perlu unik di dalam satu batch, karena batch lama tetap disimpan.
            b.HasIndex(x => new { x.ImportBatchId, x.Slug }).IsUnique();

            b.HasOne(x => x.ImportBatch)
                .WithMany(x => x.Worksheets)
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class WorksheetRowConfiguration : IEntityTypeConfiguration<WorksheetRow>
    {
        public void Configure(EntityTypeBuilder<WorksheetRow> b)
        {
            b.ToTable("WorksheetRows");
            b.HasKey(x => x.Id);

            b.Property(x => x.CellsJson).HasColumnType("nvarchar(max)").IsRequired();

            b.HasIndex(x => new { x.WorksheetId, x.RowIndex });

            b.HasOne(x => x.Worksheet)
                .WithMany(x => x.Rows)
                .HasForeignKey(x => x.WorksheetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
