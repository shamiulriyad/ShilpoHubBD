using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Data.Configurations;

public class InvestmentDocumentConfiguration : IEntityTypeConfiguration<InvestmentDocument>
{
    public void Configure(EntityTypeBuilder<InvestmentDocument> builder)
    {
        builder.ToTable("InvestmentDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(d => d.UploadedAt).IsRequired();
    }
}
