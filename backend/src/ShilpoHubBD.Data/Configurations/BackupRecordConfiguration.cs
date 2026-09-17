using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Configurations;

public class BackupRecordConfiguration : IEntityTypeConfiguration<BackupRecord>
{
    public void Configure(EntityTypeBuilder<BackupRecord> builder)
    {
        builder.ToTable("BackupRecords");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.FilePath).HasMaxLength(1000);
        builder.Property(b => b.ErrorMessage).HasMaxLength(2000);
        builder.Property(b => b.StartedAt).IsRequired();

        builder.HasIndex(b => b.StartedAt);
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.RequestedBy)
            .WithMany()
            .HasForeignKey(b => b.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
