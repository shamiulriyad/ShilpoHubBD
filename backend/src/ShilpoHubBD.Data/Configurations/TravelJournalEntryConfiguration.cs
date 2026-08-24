using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Data.Configurations;

public class TravelJournalEntryConfiguration : IEntityTypeConfiguration<TravelJournalEntry>
{
    public void Configure(EntityTypeBuilder<TravelJournalEntry> builder)
    {
        builder.ToTable("TravelJournalEntries");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title).IsRequired().HasMaxLength(150);
        builder.Property(j => j.Content).IsRequired().HasMaxLength(4000);
        builder.Property(j => j.PhotoUrl).HasMaxLength(500);
        builder.Property(j => j.CreatedAt).IsRequired();
        builder.Property(j => j.UpdatedAt).IsRequired();

        builder.HasIndex(j => j.UserId);

        builder.HasOne(j => j.User)
            .WithMany()
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(j => j.HeritagePlace)
            .WithMany()
            .HasForeignKey(j => j.HeritagePlaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(j => j.CheckIn)
            .WithMany()
            .HasForeignKey(j => j.CheckInId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
