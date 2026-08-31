using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageIndexRecordConfiguration : IEntityTypeConfiguration<HeritageIndexRecord>
{
    public void Configure(EntityTypeBuilder<HeritageIndexRecord> builder)
    {
        builder.ToTable("HeritageIndexRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.IndexType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Scope).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.ScopeLabel).IsRequired().HasMaxLength(160);
        builder.Property(r => r.Score).HasColumnType("numeric(6,2)");
        builder.Property(r => r.Rating).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Method).IsRequired().HasMaxLength(80);
        builder.Property(r => r.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.SignalsJson).HasColumnType("text");
        builder.Property(r => r.Notes).HasMaxLength(2000);
        builder.Property(r => r.PeriodStart).IsRequired();
        builder.Property(r => r.PeriodEnd).IsRequired();
        builder.Property(r => r.ComputedAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.IndexType);
        builder.HasIndex(r => new { r.IndexType, r.Scope, r.ScopeId, r.PeriodEnd });
        builder.HasIndex(r => r.ScopeLabel);
        builder.HasIndex(r => r.GeneratedByUserId);

        builder.HasOne(r => r.GeneratedBy)
            .WithMany()
            .HasForeignKey(r => r.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Components)
            .WithOne(c => c.Record)
            .HasForeignKey(c => c.HeritageIndexRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HeritageIndexComponentConfiguration : IEntityTypeConfiguration<HeritageIndexComponent>
{
    public void Configure(EntityTypeBuilder<HeritageIndexComponent> builder)
    {
        builder.ToTable("HeritageIndexComponents");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Key).IsRequired().HasMaxLength(80);
        builder.Property(c => c.Label).IsRequired().HasMaxLength(160);
        builder.Property(c => c.RawValue).HasColumnType("numeric(18,2)");
        builder.Property(c => c.Weight).HasColumnType("numeric(6,3)");
        builder.Property(c => c.ContributionScore).HasColumnType("numeric(6,2)");
        builder.Property(c => c.Detail).HasMaxLength(500);

        builder.HasIndex(c => new { c.HeritageIndexRecordId, c.DisplayOrder });
    }
}
