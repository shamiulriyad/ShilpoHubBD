using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class PartnershipStatusEventConfiguration : IEntityTypeConfiguration<PartnershipStatusEvent>
{
    public void Configure(EntityTypeBuilder<PartnershipStatusEvent> builder)
    {
        builder.ToTable("PartnershipStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
