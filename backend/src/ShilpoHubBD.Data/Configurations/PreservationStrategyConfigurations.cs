using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Configurations;

public class PreservationStrategyConfiguration : IEntityTypeConfiguration<PreservationStrategy>
{
    public void Configure(EntityTypeBuilder<PreservationStrategy> builder)
    {
        builder.ToTable("PreservationStrategies");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.HeritageProblem).IsRequired().HasMaxLength(6000);
        builder.Property(s => s.ProposedSolution).IsRequired().HasMaxLength(6000);
        builder.Property(s => s.ExpectedImpact).HasMaxLength(4000);
        builder.Property(s => s.EvidenceReferences).HasMaxLength(6000);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.OwnerUserId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ResearchProjectId);

        builder.HasOne(s => s.Owner).WithMany().HasForeignKey(s => s.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.ResearchProject).WithMany().HasForeignKey(s => s.ResearchProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(s => s.HeritageDataset).WithMany().HasForeignKey(s => s.HeritageDatasetId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Objectives).WithOne(o => o.Strategy)
            .HasForeignKey(o => o.PreservationStrategyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Actions).WithOne(a => a.Strategy)
            .HasForeignKey(a => a.PreservationStrategyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class StrategyObjectiveConfiguration : IEntityTypeConfiguration<StrategyObjective>
{
    public void Configure(EntityTypeBuilder<StrategyObjective> builder)
    {
        builder.ToTable("StrategyObjectives");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title).IsRequired().HasMaxLength(300);
        builder.Property(o => o.Description).HasMaxLength(4000);
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasIndex(o => new { o.PreservationStrategyId, o.OrderIndex });

        builder.HasMany(o => o.Actions).WithOne(a => a.Objective)
            .HasForeignKey(a => a.StrategyObjectiveId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class StrategyActionConfiguration : IEntityTypeConfiguration<StrategyAction>
{
    public void Configure(EntityTypeBuilder<StrategyAction> builder)
    {
        builder.ToTable("StrategyActions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Description).HasMaxLength(4000);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => new { a.PreservationStrategyId, a.OrderIndex });
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.AssignedToUserId);

        builder.HasOne(a => a.AssignedTo).WithMany()
            .HasForeignKey(a => a.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
