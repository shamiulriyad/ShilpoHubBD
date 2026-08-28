using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Data.Configurations;

public class ManufacturingMilestoneConfiguration : IEntityTypeConfiguration<ManufacturingMilestone>
{
    public void Configure(EntityTypeBuilder<ManufacturingMilestone> builder)
    {
        builder.ToTable("ManufacturingMilestones");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
    }
}
