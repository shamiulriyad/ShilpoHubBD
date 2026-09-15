using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Admin;

namespace ShilpoHubBD.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Module).IsRequired().HasMaxLength(60);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasData(PermissionCodes.Catalogue.Select((p, i) => new
        {
            Id = DeterministicId(i),
            p.Code,
            p.Name,
            p.Module,
            Description = (string?)p.Description,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        }));
    }

    private static Guid DeterministicId(int index)
        => Guid.Parse($"10000000-0000-0000-0000-{index:D12}");
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.GrantedAt).IsRequired();

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        builder.HasOne(rp => rp.Role)
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.GrantedBy)
            .WithMany()
            .HasForeignKey(rp => rp.GrantedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class IdentityVerificationRequestConfiguration : IEntityTypeConfiguration<IdentityVerificationRequest>
{
    public void Configure(EntityTypeBuilder<IdentityVerificationRequest> builder)
    {
        builder.ToTable("IdentityVerificationRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.DocumentNumber).IsRequired().HasMaxLength(100);
        builder.Property(r => r.FrontImageUrl).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.BackImageUrl).HasMaxLength(2000);
        builder.Property(r => r.SelfieImageUrl).HasMaxLength(2000);
        builder.Property(r => r.ApplicantNote).HasMaxLength(1000);
        builder.Property(r => r.RejectionReason).HasMaxLength(1000);
        builder.Property(r => r.SubmittedAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.SubmittedAt);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedBy)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
