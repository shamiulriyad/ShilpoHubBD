using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageCheckInConfiguration : IEntityTypeConfiguration<HeritageCheckIn>
{
    public void Configure(EntityTypeBuilder<HeritageCheckIn> builder)
    {
        builder.ToTable("HeritageCheckIns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CheckInDate).IsRequired();
        builder.Property(c => c.CheckedInAt).IsRequired();

        builder.HasIndex(c => new { c.UserId, c.HeritagePlaceId, c.CheckInDate }).IsUnique();
        builder.HasIndex(c => c.HeritagePlaceId);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.HeritagePlace)
            .WithMany()
            .HasForeignKey(c => c.HeritagePlaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
