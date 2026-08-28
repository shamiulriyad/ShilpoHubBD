using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Configurations;

public class SponsorshipImpactRecordConfiguration : IEntityTypeConfiguration<SponsorshipImpactRecord>
{
    public void Configure(EntityTypeBuilder<SponsorshipImpactRecord> builder)
    {
        builder.ToTable("SponsorshipImpactRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Description).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Metric).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Value).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(r => r.RecordedAt).IsRequired();
    }
}
