using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Traceability;

namespace ShilpoHubBD.Data.Configurations;

public class MaterialSourceConfiguration : IEntityTypeConfiguration<MaterialSource>
{
    public void Configure(EntityTypeBuilder<MaterialSource> builder)
    {
        builder.ToTable("MaterialSources");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MaterialName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.SourceLocation).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(1000);

        builder.HasIndex(m => m.ProductTraceabilityId);
    }
}
