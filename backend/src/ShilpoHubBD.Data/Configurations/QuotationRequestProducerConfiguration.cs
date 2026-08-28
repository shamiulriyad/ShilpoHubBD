using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Configurations;

public class QuotationRequestProducerConfiguration : IEntityTypeConfiguration<QuotationRequestProducer>
{
    public void Configure(EntityTypeBuilder<QuotationRequestProducer> builder)
    {
        builder.ToTable("QuotationRequestProducers");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.InvitedAt).IsRequired();

        builder.HasIndex(r => new { r.QuotationRequestId, r.ProducerId }).IsUnique();
        builder.HasIndex(r => r.ProducerId);

        builder.HasOne(r => r.Producer)
            .WithMany()
            .HasForeignKey(r => r.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Response)
            .WithOne(res => res.QuotationRequestProducer)
            .HasForeignKey<QuotationResponse>(res => res.QuotationRequestProducerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
