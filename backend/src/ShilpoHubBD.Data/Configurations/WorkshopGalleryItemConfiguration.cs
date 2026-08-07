using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class WorkshopGalleryItemConfiguration : IEntityTypeConfiguration<WorkshopGalleryItem>
{
    public void Configure(EntityTypeBuilder<WorkshopGalleryItem> builder)
    {
        builder.ToTable("WorkshopGalleryItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.MediaUrl).IsRequired().HasMaxLength(2000);
        builder.Property(i => i.MediaType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Caption).HasMaxLength(500);
        builder.Property(i => i.DisplayOrder).IsRequired();
        builder.Property(i => i.CreatedAt).IsRequired();

        builder.HasIndex(i => i.ProducerId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
