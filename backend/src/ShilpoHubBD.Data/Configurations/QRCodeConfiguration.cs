using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.QRVerification;

namespace ShilpoHubBD.Data.Configurations;

public class QRCodeConfiguration : IEntityTypeConfiguration<QRCode>
{
    public void Configure(EntityTypeBuilder<QRCode> builder)
    {
        builder.ToTable("QRCodes");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Code).IsRequired().HasMaxLength(100);
        builder.Property(q => q.CreatedAt).IsRequired();

        builder.HasIndex(q => q.Code).IsUnique();
        builder.HasIndex(q => q.ProductId);

        builder.HasOne(q => q.Product)
            .WithMany()
            .HasForeignKey(q => q.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.VerificationRecords)
            .WithOne(r => r.QRCode)
            .HasForeignKey(r => r.QRCodeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
