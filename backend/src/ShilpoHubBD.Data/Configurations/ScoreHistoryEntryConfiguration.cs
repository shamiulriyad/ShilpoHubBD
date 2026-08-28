using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Data.Configurations;

public class ScoreHistoryEntryConfiguration : IEntityTypeConfiguration<ScoreHistoryEntry>
{
    public void Configure(EntityTypeBuilder<ScoreHistoryEntry> builder)
    {
        builder.ToTable("HeritageScoreHistory");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CalculatedAt).IsRequired();

        builder.HasIndex(e => new { e.ProducerHeritageIdentityId, e.CalculatedAt });
    }
}
