using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerPartnershipStatusEventConfiguration : IEntityTypeConfiguration<ProducerPartnershipStatusEvent>
{
    public void Configure(EntityTypeBuilder<ProducerPartnershipStatusEvent> builder)
    {
        builder.ToTable("ProducerPartnershipStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasOne(e => e.ChangedBy)
            .WithMany()
            .HasForeignKey(e => e.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
