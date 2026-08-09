using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Data.Configurations;

public class ContractDocumentConfiguration : IEntityTypeConfiguration<ContractDocument>
{
    public void Configure(EntityTypeBuilder<ContractDocument> builder)
    {
        builder.ToTable("ContractDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(d => d.UploadedAt).IsRequired();
    }
}
