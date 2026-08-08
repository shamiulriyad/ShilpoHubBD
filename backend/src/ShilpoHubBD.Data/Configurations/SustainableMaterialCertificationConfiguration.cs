using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Data.Configurations;

public class SustainableMaterialCertificationConfiguration : IEntityTypeConfiguration<SustainableMaterialCertification>
{
    public void Configure(EntityTypeBuilder<SustainableMaterialCertification> builder)
    {
        builder.ToTable("SustainableMaterialCertifications");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.MaterialName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CertifyingBody).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CertificateReference).IsRequired().HasMaxLength(200);
        builder.Property(c => c.IssuedAt).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.SustainabilityProfileId);
    }
}
