using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Configurations;

public class InnovationExperimentConfiguration : IEntityTypeConfiguration<InnovationExperiment>
{
    public void Configure(EntityTypeBuilder<InnovationExperiment> builder)
    {
        builder.ToTable("InnovationExperiments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Objective).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Description).HasMaxLength(4000);
        builder.Property(e => e.ModelType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Framework).HasMaxLength(100);
        builder.Property(e => e.ConfigJson).HasMaxLength(16000);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.OwnerUserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ResearchProjectId);

        builder.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ResearchProject).WithMany().HasForeignKey(e => e.ResearchProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.HeritageDataset).WithMany().HasForeignKey(e => e.HeritageDatasetId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Versions).WithOne(v => v.Experiment)
            .HasForeignKey(v => v.InnovationExperimentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Runs).WithOne(r => r.Experiment)
            .HasForeignKey(r => r.InnovationExperimentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.CurrentVersion).WithMany()
            .HasForeignKey(e => e.CurrentVersionId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class InnovationExperimentVersionConfiguration : IEntityTypeConfiguration<InnovationExperimentVersion>
{
    public void Configure(EntityTypeBuilder<InnovationExperimentVersion> builder)
    {
        builder.ToTable("InnovationExperimentVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Label).HasMaxLength(50);
        builder.Property(v => v.Notes).IsRequired().HasMaxLength(4000);
        builder.Property(v => v.ConfigJson).IsRequired().HasMaxLength(16000);
        builder.Property(v => v.Framework).HasMaxLength(100);
        builder.Property(v => v.ArtifactUrl).HasMaxLength(2048);
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex(v => new { v.InnovationExperimentId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.CreatedBy).WithMany().HasForeignKey(v => v.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingRunConfiguration : IEntityTypeConfiguration<TrainingRun>
{
    public void Configure(EntityTypeBuilder<TrainingRun> builder)
    {
        builder.ToTable("TrainingRuns");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.DatasetSnapshotName).HasMaxLength(300);
        builder.Property(r => r.HyperparametersJson).HasMaxLength(16000);
        builder.Property(r => r.MetricsJson).HasMaxLength(16000);
        builder.Property(r => r.PrimaryMetricName).HasMaxLength(100);
        builder.Property(r => r.Notes).HasMaxLength(4000);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.InnovationExperimentId, r.RunNumber }).IsUnique();
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.ExperimentVersion).WithMany()
            .HasForeignKey(r => r.ExperimentVersionId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.TriggeredBy).WithMany()
            .HasForeignKey(r => r.TriggeredByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
