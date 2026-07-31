using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Data.Configurations
{
    public class ExecutiveMeasureConfiguration : IEntityTypeConfiguration<ExecutiveMeasure>
    {
        public void Configure(EntityTypeBuilder<ExecutiveMeasure> b)
        {
            b.ToTable("ExecutiveMeasures");
            b.HasKey(x => x.Id);

            b.Property(x => x.MetricCode).HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.MetricCode).IsUnique();

            b.Property(x => x.MetricName).HasMaxLength(200);
            b.Property(x => x.Numerator).HasPrecision(12, 2);
            b.Property(x => x.Denominator).HasPrecision(12, 2);
            b.Property(x => x.ScorePercent).HasPrecision(9, 4);
            b.Property(x => x.TargetPercent).HasPrecision(9, 4);
            b.Property(x => x.Status).HasMaxLength(50);
            b.Property(x => x.Notes).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class QuickFactConfiguration : IEntityTypeConfiguration<QuickFact>
    {
        public void Configure(EntityTypeBuilder<QuickFact> b)
        {
            b.ToTable("QuickFacts");
            b.HasKey(x => x.Id);

            b.Property(x => x.FactName).HasMaxLength(200).IsRequired();
            b.Property(x => x.FactValue).HasMaxLength(200);
            b.Property(x => x.Icon).HasMaxLength(50);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.DisplayOrder);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ClsrHealthMapRowConfiguration : IEntityTypeConfiguration<ClsrHealthMapRow>
    {
        public void Configure(EntityTypeBuilder<ClsrHealthMapRow> b)
        {
            b.ToTable("ClsrHealthMapRows");
            b.HasKey(x => x.Id);

            b.Property(x => x.ClsrId).HasMaxLength(50).IsRequired();
            b.Property(x => x.ClsrDescription).HasMaxLength(300);
            b.Property(x => x.HealthStatus).HasMaxLength(50);

            foreach (var status in new[] { nameof(ClsrHealthMapRow.Zona11Status), nameof(ClsrHealthMapRow.Zona12Status), nameof(ClsrHealthMapRow.Zona13Status), nameof(ClsrHealthMapRow.Zona14Status) })
            {
                b.Property(status).HasMaxLength(50);
            }

            foreach (var score in new[] { nameof(ClsrHealthMapRow.Zona11Score), nameof(ClsrHealthMapRow.Zona12Score), nameof(ClsrHealthMapRow.Zona13Score), nameof(ClsrHealthMapRow.Zona14Score), nameof(ClsrHealthMapRow.Regional4Score) })
            {
                b.Property(score).HasPrecision(9, 4);
            }

            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.ClsrId);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TopFiveItemConfiguration : IEntityTypeConfiguration<TopFiveItem>
    {
        public void Configure(EntityTypeBuilder<TopFiveItem> b)
        {
            b.ToTable("TopFiveItems");
            b.HasKey(x => x.Id);

            b.Property(x => x.Category).HasMaxLength(100).IsRequired();
            b.Property(x => x.Item).HasMaxLength(300);
            b.Property(x => x.Percent).HasPrecision(9, 6);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => new { x.Category, x.DisplayOrder });

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TrendPointConfiguration : IEntityTypeConfiguration<TrendPoint>
    {
        public void Configure(EntityTypeBuilder<TrendPoint> b)
        {
            b.ToTable("TrendPoints");
            b.HasKey(x => x.Id);

            b.Property(x => x.MonthLabel).HasMaxLength(20);
            b.Property(x => x.ActualPercent).HasPrecision(9, 4);
            b.Property(x => x.PlannedPercent).HasPrecision(9, 4);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.PeriodMonth).IsUnique();

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ZonaScoreConfiguration : IEntityTypeConfiguration<ZonaScore>
    {
        public void Configure(EntityTypeBuilder<ZonaScore> b)
        {
            b.ToTable("ZonaScores");
            b.HasKey(x => x.Id);

            b.Property(x => x.ZonaLabel).HasMaxLength(50);
            b.Property(x => x.ScorePercent).HasPrecision(9, 4);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.Zona).IsUnique();

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DashboardTextConfiguration : IEntityTypeConfiguration<DashboardText>
    {
        public void Configure(EntityTypeBuilder<DashboardText> b)
        {
            b.ToTable("DashboardTexts");
            b.HasKey(x => x.Id);

            b.Property(x => x.Section).HasMaxLength(150).IsRequired();
            b.HasIndex(x => x.Section).IsUnique();

            b.Property(x => x.Text).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
