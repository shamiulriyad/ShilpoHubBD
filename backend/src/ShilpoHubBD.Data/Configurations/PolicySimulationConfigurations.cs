using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class PolicySimulationConfiguration : IEntityTypeConfiguration<PolicySimulation>
{
    public void Configure(EntityTypeBuilder<PolicySimulation> builder)
    {
        builder.ToTable("PolicySimulations");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(160);
        builder.Property(s => s.SimulationType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.Scope).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ScopeLabel).IsRequired().HasMaxLength(160);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.InputsJson).IsRequired().HasColumnType("text");
        builder.Property(s => s.AssumptionsJson).HasColumnType("text");
        builder.Property(s => s.Method).IsRequired(false).HasMaxLength(80);
        builder.Property(s => s.Summary).IsRequired(false).HasMaxLength(2000);
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.FailureReason).HasMaxLength(1000);
        builder.Property(s => s.BaselineExportValue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.BaselineTourismRevenue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.BaselineEconomyValue).HasColumnType("numeric(18,2)");
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.SimulationType);
        builder.HasIndex(s => new { s.Scope, s.ScopeId });
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.GeneratedByUserId);
        builder.HasIndex(s => s.CreatedAt);

        builder.HasOne(s => s.GeneratedBy)
            .WithMany()
            .HasForeignKey(s => s.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Projections)
            .WithOne(p => p.Simulation)
            .HasForeignKey(p => p.PolicySimulationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Recommendations)
            .WithOne(r => r.Simulation)
            .HasForeignKey(r => r.PolicySimulationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PolicySimulationProjectionConfiguration : IEntityTypeConfiguration<PolicySimulationProjection>
{
    public void Configure(EntityTypeBuilder<PolicySimulationProjection> builder)
    {
        builder.ToTable("PolicySimulationProjections");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Metric).IsRequired().HasMaxLength(80);
        builder.Property(p => p.Unit).IsRequired().HasMaxLength(20);
        builder.Property(p => p.BaselineValue).HasColumnType("numeric(18,2)");
        builder.Property(p => p.ProjectedValue).HasColumnType("numeric(18,2)");
        builder.Property(p => p.DeltaValue).HasColumnType("numeric(18,2)");
        builder.Property(p => p.Confidence).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Detail).HasMaxLength(500);

        builder.HasIndex(p => new { p.PolicySimulationId, p.DisplayOrder });
    }
}

public class PolicySimulationRecommendationConfiguration : IEntityTypeConfiguration<PolicySimulationRecommendation>
{
    public void Configure(EntityTypeBuilder<PolicySimulationRecommendation> builder)
    {
        builder.ToTable("PolicySimulationRecommendations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Detail).IsRequired().HasMaxLength(1000);

        builder.HasIndex(r => new { r.PolicySimulationId, r.DisplayOrder });
    }
}
