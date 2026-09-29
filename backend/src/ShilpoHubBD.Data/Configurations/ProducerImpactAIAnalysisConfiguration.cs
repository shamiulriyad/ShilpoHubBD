using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerImpactAIAnalysisConfiguration : IEntityTypeConfiguration<ProducerImpactAIAnalysis>
{
    public void Configure(EntityTypeBuilder<ProducerImpactAIAnalysis> b)
    {
        b.ToTable("ProducerImpactAIAnalyses");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProviderName).IsRequired().HasMaxLength(40);
        b.Property(x => x.GeneratedAt).IsRequired();

        b.HasIndex(x => x.CaseId);
        b.HasIndex(x => x.ImpactAssessmentId);

        b.HasOne(x => x.Case).WithMany().HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ImpactAssessment).WithMany().HasForeignKey(x => x.ImpactAssessmentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.RequestedBy).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Findings).WithOne(f => f.Analysis).HasForeignKey(f => f.AnalysisId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProducerImpactAIFindingConfiguration : IEntityTypeConfiguration<ProducerImpactAIFinding>
{
    public void Configure(EntityTypeBuilder<ProducerImpactAIFinding> b)
    {
        b.ToTable("ProducerImpactAIFindings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Text).IsRequired().HasMaxLength(2000);
    }
}
