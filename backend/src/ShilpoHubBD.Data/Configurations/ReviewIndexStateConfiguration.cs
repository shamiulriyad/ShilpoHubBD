using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Configurations;

public class ReviewIndexStateConfiguration : IEntityTypeConfiguration<ReviewIndexState>
{
    public void Configure(EntityTypeBuilder<ReviewIndexState> builder)
    {
        builder.ToTable("ReviewIndexStates");
        builder.HasKey(s => s.ReviewId);
        builder.Property(s => s.ReviewId).ValueGeneratedNever();   // = the review id; deliberately no FK (survives review deletion)
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.TextHash).HasMaxLength(64);
        builder.Property(s => s.EmbeddingModel).HasMaxLength(100);
        builder.Property(s => s.LastError).HasMaxLength(1000);
        builder.Property(s => s.UpdatedAt).IsRequired();

        // The worker only polls unsynced rows.
        builder.HasIndex(s => new { s.Status, s.UpdatedAt }).HasFilter("\"Status\" <> 'Synced'").HasDatabaseName("IX_ReviewIndexStates_Pending");
    }
}
