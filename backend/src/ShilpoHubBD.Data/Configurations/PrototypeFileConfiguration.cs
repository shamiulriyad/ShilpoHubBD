using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Configurations;

public class PrototypeFileConfiguration : IEntityTypeConfiguration<PrototypeFile>
{
    public void Configure(EntityTypeBuilder<PrototypeFile> builder)
    {
        builder.ToTable("PrototypeFiles");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(f => f.FileType).IsRequired().HasMaxLength(50);
        builder.Property(f => f.UploadedAt).IsRequired();
    }
}
