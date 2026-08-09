using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Configurations;

public class QuotationResponseConfiguration : IEntityTypeConfiguration<QuotationResponse>
{
    public void Configure(EntityTypeBuilder<QuotationResponse> builder)
    {
        builder.ToTable("QuotationResponses");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalPrice).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(2000);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.DecisionNotes).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.QuotationRequestProducerId).IsUnique();

        builder.HasMany(r => r.Items)
            .WithOne(i => i.QuotationResponse)
            .HasForeignKey(i => i.QuotationResponseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
