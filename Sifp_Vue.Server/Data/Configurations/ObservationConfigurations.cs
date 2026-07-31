using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sifp_Vue.Server.Models.Entities;

namespace Sifp_Vue.Server.Data.Configurations
{
    /// <summary>
    /// Semua FK ke <see cref="ImportBatch"/> memakai <c>Restrict</c>, bukan Cascade/SetNull.
    /// Alasannya: SQL Server menolak dua jalur cascade menuju tabel yang sama
    /// (ImportBatch → Observation → SifQuestion dan ImportBatch → SifQuestion).
    /// Penghapusan data lama dilakukan eksplisit oleh ExcelImportService.
    /// </summary>
    public class ObservationConfiguration : IEntityTypeConfiguration<Observation>
    {
        public void Configure(EntityTypeBuilder<Observation> b)
        {
            b.ToTable("Observations");
            b.HasKey(x => x.Id);

            b.Property(x => x.ObsCode).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.ObsCode).IsUnique();

            b.Property(x => x.ProtocolCode).HasMaxLength(50);
            b.Property(x => x.ProtocolName).HasMaxLength(200);
            b.Property(x => x.Site).HasMaxLength(200);
            b.Property(x => x.AreaEquipment).HasMaxLength(200);
            b.Property(x => x.Activity).HasMaxLength(300);
            b.Property(x => x.Company).HasMaxLength(200);
            b.Property(x => x.Observer1).HasMaxLength(150);
            b.Property(x => x.Observer2).HasMaxLength(150);
            b.Property(x => x.Observer3).HasMaxLength(150);
            b.Property(x => x.Status).HasMaxLength(50);
            b.Property(x => x.PerformancePercent).HasPrecision(9, 4);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.Zona);
            b.HasIndex(x => x.ObservationDate);
            b.HasIndex(x => x.ProtocolCode);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class SifQuestionConfiguration : IEntityTypeConfiguration<SifQuestion>
    {
        public void Configure(EntityTypeBuilder<SifQuestion> b)
        {
            b.ToTable("SifQuestions");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProtocolCode).HasMaxLength(50);
            b.Property(x => x.ProtocolName).HasMaxLength(200);
            b.Property(x => x.QuestionRef).HasMaxLength(20);
            b.Property(x => x.CcvcId).HasMaxLength(50);
            b.Property(x => x.QuestionText).HasMaxLength(1000);
            b.Property(x => x.Answer).HasMaxLength(5).IsRequired();
            b.Property(x => x.Comments).HasColumnType("nvarchar(max)");
            b.Property(x => x.SifExposure).HasMaxLength(200);
            b.Property(x => x.CriticalSafeguard).HasMaxLength(200);
            b.Property(x => x.Site).HasMaxLength(200);
            b.Property(x => x.Activity).HasMaxLength(300);
            b.Property(x => x.Company).HasMaxLength(200);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.CcvcId);
            b.HasIndex(x => x.Answer);

            b.HasOne(x => x.Observation)
                .WithMany(x => x.SifQuestions)
                .HasForeignKey(x => x.ObservationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CcvcLibraryItemConfiguration : IEntityTypeConfiguration<CcvcLibraryItem>
    {
        public void Configure(EntityTypeBuilder<CcvcLibraryItem> b)
        {
            b.ToTable("CcvcLibraryItems");
            b.HasKey(x => x.Id);

            b.Property(x => x.CcvcId).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.CcvcId).IsUnique();

            b.Property(x => x.ProtocolGroup).HasMaxLength(150);
            b.Property(x => x.PsecId).HasMaxLength(50);
            b.Property(x => x.PsecName).HasMaxLength(200);
            b.Property(x => x.ExposureType).HasMaxLength(150);
            b.Property(x => x.QuestionCode).HasMaxLength(20);
            b.Property(x => x.QuestionSummary).HasMaxLength(500);
            b.Property(x => x.VerificationPurpose).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.PsecId);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ErrorTrapConfiguration : IEntityTypeConfiguration<ErrorTrap>
    {
        public void Configure(EntityTypeBuilder<ErrorTrap> b)
        {
            b.ToTable("ErrorTraps");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProtocolCode).HasMaxLength(50);
            b.Property(x => x.ProtocolName).HasMaxLength(200);
            b.Property(x => x.Category).HasMaxLength(100);
            b.Property(x => x.TrapName).HasMaxLength(200);
            b.Property(x => x.Comments).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.Category);

            b.HasOne(x => x.Observation)
                .WithMany(x => x.ErrorTraps)
                .HasForeignKey(x => x.ObservationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class HpToolConfiguration : IEntityTypeConfiguration<HpTool>
    {
        public void Configure(EntityTypeBuilder<HpTool> b)
        {
            b.ToTable("HpTools");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProtocolCode).HasMaxLength(50);
            b.Property(x => x.ProtocolName).HasMaxLength(200);
            b.Property(x => x.ToolName).HasMaxLength(200);
            b.Property(x => x.Tujuan).HasColumnType("nvarchar(max)");
            b.Property(x => x.KapanDigunakan).HasColumnType("nvarchar(max)");
            b.Property(x => x.CaraPakai).HasColumnType("nvarchar(max)");
            b.Property(x => x.EffectivenessNotes).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.ToolName);

            b.HasOne(x => x.Observation)
                .WithMany(x => x.HpTools)
                .HasForeignKey(x => x.ObservationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DriftConditionConfiguration : IEntityTypeConfiguration<DriftCondition>
    {
        public void Configure(EntityTypeBuilder<DriftCondition> b)
        {
            b.ToTable("DriftConditions");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProtocolCode).HasMaxLength(50);
            b.Property(x => x.ProtocolName).HasMaxLength(200);
            b.Property(x => x.Situation).HasColumnType("nvarchar(max)");
            b.Property(x => x.Level1).HasMaxLength(200);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.Level2).HasMaxLength(200);
            b.Property(x => x.Reason).HasColumnType("nvarchar(max)");
            b.Property(x => x.Status).HasMaxLength(50);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.Code);

            b.HasOne(x => x.Observation)
                .WithMany(x => x.DriftConditions)
                .HasForeignKey(x => x.ObservationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class LatentConditionConfiguration : IEntityTypeConfiguration<LatentCondition>
    {
        public void Configure(EntityTypeBuilder<LatentCondition> b)
        {
            b.ToTable("LatentConditions");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProtocolCode).HasMaxLength(50);
            b.Property(x => x.ProtocolName).HasMaxLength(200);
            b.Property(x => x.ObservationText).HasColumnType("nvarchar(max)");
            b.Property(x => x.Level1).HasMaxLength(200);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.Level2).HasMaxLength(200);
            b.Property(x => x.Reason).HasColumnType("nvarchar(max)");
            b.Property(x => x.Status).HasMaxLength(50);
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.Code);

            b.HasOne(x => x.Observation)
                .WithMany(x => x.LatentConditions)
                .HasForeignKey(x => x.ObservationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ImprovementInitiativeConfiguration : IEntityTypeConfiguration<ImprovementInitiative>
    {
        public void Configure(EntityTypeBuilder<ImprovementInitiative> b)
        {
            b.ToTable("ImprovementInitiatives");
            b.HasKey(x => x.Id);

            b.Property(x => x.ImprovementCode).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.ImprovementCode).IsUnique();

            b.Property(x => x.Initiative).HasMaxLength(300);
            b.Property(x => x.RelatedClsr).HasMaxLength(200);
            b.Property(x => x.Owner).HasMaxLength(150);
            b.Property(x => x.Status).HasMaxLength(50);
            b.Property(x => x.ExpectedImpact).HasColumnType("nvarchar(max)");
            b.Property(x => x.Notes).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(100);
            b.Property(x => x.UpdatedBy).HasMaxLength(100);

            b.HasIndex(x => x.Status);

            b.HasOne(x => x.ImportBatch)
                .WithMany()
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
