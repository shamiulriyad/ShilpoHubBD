using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class CulturalEventConfiguration : IEntityTypeConfiguration<CulturalEvent>
{
    public void Configure(EntityTypeBuilder<CulturalEvent> builder)
    {
        builder.ToTable("CulturalEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ImageUrl).HasMaxLength(1000);
        builder.Property(e => e.EventDate).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsActive);

        builder.HasOne(e => e.District)
            .WithMany()
            .HasForeignKey(e => e.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
