using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Configurations;

public class TransportOptionConfiguration : IEntityTypeConfiguration<TransportOption>
{
    public void Configure(EntityTypeBuilder<TransportOption> builder)
    {
        builder.ToTable("TransportOptions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Mode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.OriginName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.DestinationDistrict).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Operator).IsRequired().HasMaxLength(150);
        builder.Property(t => t.ServiceName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ServiceClasses).HasMaxLength(300);
        builder.Property(t => t.Schedule).HasMaxLength(300);
        builder.Property(t => t.DurationText).HasMaxLength(150);
        builder.Property(t => t.FareBdt).HasColumnType("decimal(18,2)");
        builder.Property(t => t.FareNote).HasMaxLength(600);
        builder.Property(t => t.BookingUrl).HasMaxLength(500);
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.Property(t => t.SourceUrl).IsRequired().HasMaxLength(500);
        builder.Property(t => t.VerificationStatus).IsRequired().HasMaxLength(20);
        builder.Property(t => t.UnverifiedFields).HasMaxLength(1000);

        builder.HasIndex(t => new { t.DestinationDistrict, t.Mode });
        builder.HasIndex(t => new { t.OriginName, t.DestinationDistrict, t.Mode, t.ServiceName }).IsUnique();
    }
}
