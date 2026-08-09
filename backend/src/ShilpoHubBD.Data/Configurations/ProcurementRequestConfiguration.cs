using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Data.Configurations;

public class ProcurementRequestConfiguration : IEntityTypeConfiguration<ProcurementRequest>
{
    public void Configure(EntityTypeBuilder<ProcurementRequest> builder)
    {
        builder.ToTable("ProcurementRequests");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Budget).HasColumnType("numeric(18,2)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ApprovalNotes).HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ReferenceNumber).IsUnique();
        builder.HasIndex(p => p.BusinessPartnerId);
        builder.HasIndex(p => p.ProducerId);
        builder.HasIndex(p => p.OrderId).IsUnique();

        builder.HasOne(p => p.BusinessPartner)
            .WithMany()
            .HasForeignKey(p => p.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ApprovedBy)
            .WithMany()
            .HasForeignKey(p => p.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.QuotationRequestRef)
            .WithMany()
            .HasForeignKey(p => p.QuotationRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.QuotationResponseRef)
            .WithMany()
            .HasForeignKey(p => p.QuotationResponseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Items)
            .WithOne(i => i.ProcurementRequest)
            .HasForeignKey(i => i.ProcurementRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StatusHistory)
            .WithOne(h => h.ProcurementRequest)
            .HasForeignKey(h => h.ProcurementRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
