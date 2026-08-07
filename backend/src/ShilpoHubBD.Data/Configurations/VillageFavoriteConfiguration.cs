using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Configurations;

public class VillageFavoriteConfiguration : IEntityTypeConfiguration<VillageFavorite>
{
    public void Configure(EntityTypeBuilder<VillageFavorite> builder)
    {
        builder.ToTable("VillageFavorites");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => new { f.UserId, f.VillageId }).IsUnique();

        builder.HasOne(f => f.Village)
            .WithMany()
            .HasForeignKey(f => f.VillageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
