using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchAICitationConfiguration : IEntityTypeConfiguration<ResearchAICitation>
{
    public void Configure(EntityTypeBuilder<ResearchAICitation> builder)
    {
        builder.ToTable("ResearchAICitations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Style).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.SourceTitle).IsRequired().HasMaxLength(400);
        builder.Property(c => c.Authors).HasMaxLength(2000);
        builder.Property(c => c.Container).HasMaxLength(400);
        builder.Property(c => c.Doi).HasMaxLength(200);
        builder.Property(c => c.Url).HasMaxLength(2048);
        builder.Property(c => c.FormattedCitation).IsRequired().HasMaxLength(4000);

        builder.HasIndex(c => new { c.ResearchAIAnalysisId, c.DisplayOrder });

        builder.HasOne(c => c.Publication)
            .WithMany()
            .HasForeignKey(c => c.ResearchPublicationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
