using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Data.Configurations;

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.ToTable("Badges");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(500);
        builder.Property(b => b.IconUrl).HasMaxLength(500);
        builder.Property(b => b.FestivalName).HasMaxLength(100);
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasIndex(b => b.Type);
        builder.HasIndex(b => b.DistrictId).IsUnique().HasFilter("\"DistrictId\" IS NOT NULL");

        builder.HasOne(b => b.District)
            .WithMany()
            .HasForeignKey(b => b.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
