using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Traceability;

namespace ShilpoHubBD.Data.Configurations;

public class TimelineEventConfiguration : IEntityTypeConfiguration<TimelineEvent>
{
    public void Configure(EntityTypeBuilder<TimelineEvent> builder)
    {
        builder.ToTable("TimelineEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Location).HasMaxLength(200);
        builder.Property(e => e.EventDate).IsRequired();

        builder.HasIndex(e => e.ProductTraceabilityId);
    }
}
