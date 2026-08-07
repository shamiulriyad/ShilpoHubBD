using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Configurations;

public class VillageConfiguration : IEntityTypeConfiguration<Village>
{
    public void Configure(EntityTypeBuilder<Village> builder)
    {
        builder.ToTable("Villages");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Craft).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Description).HasMaxLength(2000);
        builder.Property(v => v.ImageUrl).HasMaxLength(2000);
        builder.Property(v => v.IsActive).IsRequired();
        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.UpdatedAt).IsRequired();

        builder.HasIndex(v => v.DistrictId);

        builder.HasOne(v => v.District)
            .WithMany()
            .HasForeignKey(v => v.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
