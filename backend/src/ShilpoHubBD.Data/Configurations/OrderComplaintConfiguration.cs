using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Certificate;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Configurations;

public class OrderComplaintConfiguration : IEntityTypeConfiguration<OrderComplaint>
{
    public void Configure(EntityTypeBuilder<OrderComplaint> builder)
    {
        builder.ToTable("OrderComplaints");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Subject).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.ProducerResponse).HasMaxLength(2000);
        builder.Property(c => c.CustomerNote).HasMaxLength(1000);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(c => new { c.CustomerId, c.ProductId });
        builder.HasIndex(c => c.ProducerId);
        builder.HasIndex(c => c.OrderId);

        builder.HasOne(c => c.Order).WithMany().HasForeignKey(c => c.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Producer).WithMany().HasForeignKey(c => c.ProducerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExpertiseCertificateConfiguration : IEntityTypeConfiguration<ExpertiseCertificate>
{
    public void Configure(EntityTypeBuilder<ExpertiseCertificate> builder)
    {
        builder.ToTable("ExpertiseCertificates");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(40);
        builder.Property(c => c.Expertise).IsRequired().HasMaxLength(200);
        builder.Property(c => c.AverageRating).HasColumnType("decimal(4,2)");
        builder.Property(c => c.Level).HasConversion<string>().HasMaxLength(10).IsRequired();

        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => c.ProducerId);

        builder.HasOne(c => c.Producer).WithMany().HasForeignKey(c => c.ProducerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.IssuedBy).WithMany().HasForeignKey(c => c.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
