using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageDatasetAccessGrantConfiguration : IEntityTypeConfiguration<HeritageDatasetAccessGrant>
{
    public void Configure(EntityTypeBuilder<HeritageDatasetAccessGrant> builder)
    {
        builder.ToTable("HeritageDatasetAccessGrants");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.AccessRole).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.GrantedAt).IsRequired();

        builder.HasIndex(g => new { g.HeritageDatasetId, g.UserId }).IsUnique();
        builder.HasIndex(g => g.UserId);

        builder.HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.GrantedBy)
            .WithMany()
            .HasForeignKey(g => g.GrantedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
