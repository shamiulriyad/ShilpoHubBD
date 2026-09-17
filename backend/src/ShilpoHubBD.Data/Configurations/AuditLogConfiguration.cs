using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActorName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.ActorUserId);
    }
}
