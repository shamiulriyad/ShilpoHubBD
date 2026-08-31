using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageFestivalConfiguration : IEntityTypeConfiguration<HeritageFestival>
{
    public void Configure(EntityTypeBuilder<HeritageFestival> builder)
    {
        builder.ToTable("HeritageFestivals");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Description).IsRequired().HasMaxLength(4000);
        builder.Property(f => f.ImageUrl).HasMaxLength(1000);
        builder.Property(f => f.StartDate).IsRequired();
        builder.Property(f => f.EndDate).IsRequired();
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();

        builder.HasIndex(f => f.StartDate);
        builder.HasIndex(f => f.IsActive);

        builder.HasOne(f => f.District)
            .WithMany()
            .HasForeignKey(f => f.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
