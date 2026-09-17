using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKeys");
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name).IsRequired().HasMaxLength(200);
        builder.Property(k => k.KeyPrefix).IsRequired().HasMaxLength(20);
        builder.Property(k => k.KeyHash).IsRequired().HasMaxLength(128);
        builder.Property(k => k.CreatedAt).IsRequired();

        builder.HasIndex(k => k.KeyHash).IsUnique();
        builder.HasIndex(k => k.IsActive);

        builder.HasOne(k => k.CreatedBy)
            .WithMany()
            .HasForeignKey(k => k.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
