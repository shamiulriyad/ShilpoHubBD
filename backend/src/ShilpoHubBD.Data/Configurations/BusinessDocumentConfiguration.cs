using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Data.Configurations;

public class BusinessDocumentConfiguration : IEntityTypeConfiguration<BusinessDocument>
{
    public void Configure(EntityTypeBuilder<BusinessDocument> builder)
    {
        builder.ToTable("BusinessDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentType).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(d => d.DocumentNumber).HasMaxLength(100);
        builder.Property(d => d.UploadedAt).IsRequired();
    }
}
