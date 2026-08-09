using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Data.Configurations;

public class DesignFileConfiguration : IEntityTypeConfiguration<DesignFile>
{
    public void Configure(EntityTypeBuilder<DesignFile> builder)
    {
        builder.ToTable("DesignFiles");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(f => f.FileType).IsRequired().HasMaxLength(50);
        builder.Property(f => f.UploadedAt).IsRequired();

        builder.HasOne(f => f.UploadedBy)
            .WithMany()
            .HasForeignKey(f => f.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Revision)
            .WithMany(r => r.Files)
            .HasForeignKey(f => f.RevisionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
