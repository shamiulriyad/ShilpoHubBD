using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Configurations;

public class ProductModerationStateConfiguration : IEntityTypeConfiguration<ProductModerationState>
{
    public void Configure(EntityTypeBuilder<ProductModerationState> builder)
    {
        builder.ToTable("ProductModerationStates");
        builder.HasKey(s => s.ProductId);
        builder.Property(s => s.ProductId).ValueGeneratedNever();
        builder.Property(s => s.RiskState).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.RiskState);
    }
}

public class ProductModerationEventConfiguration : IEntityTypeConfiguration<ProductModerationEvent>
{
    public void Configure(EntityTypeBuilder<ProductModerationEvent> builder)
    {
        builder.ToTable("ProductModerationEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.ComplaintType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.SimilarReviewIds).HasColumnType("uuid[]").IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasOne(e => e.ProductModerationState)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ProductId, e.CreatedAt });
    }
}

public class ProducerModerationWarningConfiguration : IEntityTypeConfiguration<ProducerModerationWarning>
{
    public void Configure(EntityTypeBuilder<ProducerModerationWarning> builder)
    {
        builder.ToTable("ProducerModerationWarnings");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.ComplaintType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(w => w.Message).IsRequired().HasMaxLength(1000);
        builder.Property(w => w.CreatedAt).IsRequired();

        builder.HasIndex(w => w.ProductId);
        builder.HasIndex(w => w.ProducerId);
    }
}
