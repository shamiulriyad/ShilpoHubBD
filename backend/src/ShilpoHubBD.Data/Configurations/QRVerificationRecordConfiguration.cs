using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.QRVerification;

namespace ShilpoHubBD.Data.Configurations;

public class QRVerificationRecordConfiguration : IEntityTypeConfiguration<QRVerificationRecord>
{
    public void Configure(EntityTypeBuilder<QRVerificationRecord> builder)
    {
        builder.ToTable("QRVerificationRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ScannedCode).IsRequired().HasMaxLength(100);
        builder.Property(r => r.VerifiedAt).IsRequired();

        builder.HasIndex(r => r.QRCodeId);
        builder.HasIndex(r => r.VerifiedByUserId);

        builder.HasOne(r => r.VerifiedByUser)
            .WithMany()
            .HasForeignKey(r => r.VerifiedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
