using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class LocalCuisineConfiguration : IEntityTypeConfiguration<LocalCuisine>
{
    public void Configure(EntityTypeBuilder<LocalCuisine> builder)
    {
        builder.ToTable("LocalCuisines");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.WhereToTry).HasMaxLength(1000);
        builder.Property(c => c.ImageUrl).HasMaxLength(1000);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => c.IsActive);

        builder.HasOne(c => c.District)
            .WithMany()
            .HasForeignKey(c => c.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
