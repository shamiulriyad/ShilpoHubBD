using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Configurations;

public class PrototypeVersionConfiguration : IEntityTypeConfiguration<PrototypeVersion>
{
    public void Configure(EntityTypeBuilder<PrototypeVersion> builder)
    {
        builder.ToTable("PrototypeVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Description).IsRequired().HasMaxLength(2000);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.DecisionNotes).HasMaxLength(1000);
        builder.Property(v => v.SubmittedAt).IsRequired();

        builder.HasIndex(v => new { v.ProjectId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.SubmittedBy)
            .WithMany()
            .HasForeignKey(v => v.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Files)
            .WithOne(f => f.PrototypeVersion)
            .HasForeignKey(f => f.PrototypeVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
