using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Data.Configurations;

public class SustainabilityProfileConfiguration : IEntityTypeConfiguration<SustainabilityProfile>
{
    public void Configure(EntityTypeBuilder<SustainabilityProfile> builder)
    {
        builder.ToTable("SustainabilityProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.EcoScore).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(p => p.BadgeLevel).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.TotalCarbonSavingsKg).IsRequired().HasColumnType("decimal(12,2)");
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ProducerId).IsUnique();

        builder.HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.MaterialRecords)
            .WithOne(r => r.SustainabilityProfile)
            .HasForeignKey(r => r.SustainabilityProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Certifications)
            .WithOne(c => c.SustainabilityProfile)
            .HasForeignKey(c => c.SustainabilityProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
