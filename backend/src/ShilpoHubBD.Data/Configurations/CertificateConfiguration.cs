using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Certificate;

namespace ShilpoHubBD.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ProducerName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.District).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Category).IsRequired().HasMaxLength(100);
        builder.Property(c => c.IssuedAt).IsRequired();

        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => c.ProductId);
        builder.HasIndex(c => c.ProducerId);

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Producer)
            .WithMany()
            .HasForeignKey(c => c.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
