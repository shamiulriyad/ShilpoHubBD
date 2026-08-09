using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Configurations;

public class ProductDevelopmentMilestoneConfiguration : IEntityTypeConfiguration<ProductDevelopmentMilestone>
{
    public void Configure(EntityTypeBuilder<ProductDevelopmentMilestone> builder)
    {
        builder.ToTable("ProductDevelopmentMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
    }
}
