using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Configurations;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("LoginAttempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Email).IsRequired().HasMaxLength(320);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => new { a.IpAddress, a.CreatedAt });
        builder.HasIndex(a => new { a.Email, a.CreatedAt });
    }
}

public class BlockedIpAddressConfiguration : IEntityTypeConfiguration<BlockedIpAddress>
{
    public void Configure(EntityTypeBuilder<BlockedIpAddress> builder)
    {
        builder.ToTable("BlockedIpAddresses");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.IpAddress).IsRequired().HasMaxLength(64);
        builder.Property(b => b.Reason).IsRequired().HasMaxLength(500);
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasIndex(b => b.IpAddress).IsUnique();

        builder.HasOne(b => b.BlockedBy)
            .WithMany()
            .HasForeignKey(b => b.BlockedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
