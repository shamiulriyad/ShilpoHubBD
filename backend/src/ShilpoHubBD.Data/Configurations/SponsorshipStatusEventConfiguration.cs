using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Configurations;

public class SponsorshipStatusEventConfiguration : IEntityTypeConfiguration<SponsorshipStatusEvent>
{
    public void Configure(EntityTypeBuilder<SponsorshipStatusEvent> builder)
    {
        builder.ToTable("SponsorshipStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
