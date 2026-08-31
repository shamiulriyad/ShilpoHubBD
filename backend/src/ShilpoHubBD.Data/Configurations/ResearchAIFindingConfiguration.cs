using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchAIFindingConfiguration : IEntityTypeConfiguration<ResearchAIFinding>
{
    public void Configure(EntityTypeBuilder<ResearchAIFinding> builder)
    {
        builder.ToTable("ResearchAIFindings");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.Heading).IsRequired().HasMaxLength(300);
        builder.Property(f => f.Detail).IsRequired().HasMaxLength(4000);
        builder.Property(f => f.Metric).HasMaxLength(120);

        builder.HasIndex(f => new { f.ResearchAIAnalysisId, f.DisplayOrder });
    }
}
