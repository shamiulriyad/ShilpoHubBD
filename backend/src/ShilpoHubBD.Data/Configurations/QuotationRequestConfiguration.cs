using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Configurations;

public class QuotationRequestConfiguration : IEntityTypeConfiguration<QuotationRequest>
{
    public void Configure(EntityTypeBuilder<QuotationRequest> builder)
    {
        builder.ToTable("QuotationRequests");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(q => q.Title).IsRequired().HasMaxLength(200);
        builder.Property(q => q.Requirements).HasMaxLength(2000);
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.UpdatedAt).IsRequired();

        builder.HasIndex(q => q.ReferenceNumber).IsUnique();
        builder.HasIndex(q => q.BusinessPartnerId);

        builder.HasOne(q => q.BusinessPartner)
            .WithMany()
            .HasForeignKey(q => q.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Items)
            .WithOne(i => i.QuotationRequest)
            .HasForeignKey(i => i.QuotationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Recipients)
            .WithOne(r => r.QuotationRequest)
            .HasForeignKey(r => r.QuotationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.StatusHistory)
            .WithOne(h => h.QuotationRequest)
            .HasForeignKey(h => h.QuotationRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
