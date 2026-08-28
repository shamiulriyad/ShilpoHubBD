using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Data.Configurations;

public class ProcurementStatusEventConfiguration : IEntityTypeConfiguration<ProcurementStatusEvent>
{
    public void Configure(EntityTypeBuilder<ProcurementStatusEvent> builder)
    {
        builder.ToTable("ProcurementStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
