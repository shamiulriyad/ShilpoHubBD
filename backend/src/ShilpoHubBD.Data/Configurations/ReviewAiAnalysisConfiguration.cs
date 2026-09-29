using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Configurations;

public class ReviewAiAnalysisConfiguration : IEntityTypeConfiguration<ReviewAiAnalysis>
{
    public void Configure(EntityTypeBuilder<ReviewAiAnalysis> builder)
    {
        builder.ToTable("ReviewAiAnalyses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ComplaintType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.IssueSummary).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // One analysis per review, ever.
        builder.HasIndex(a => a.ReviewId).IsUnique();

        builder.HasOne(a => a.Review)
            .WithOne()
            .HasForeignKey<ReviewAiAnalysis>(a => a.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
