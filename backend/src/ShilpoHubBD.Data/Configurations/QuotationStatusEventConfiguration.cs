using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Configurations;

public class QuotationStatusEventConfiguration : IEntityTypeConfiguration<QuotationStatusEvent>
{
    public void Configure(EntityTypeBuilder<QuotationStatusEvent> builder)
    {
        builder.ToTable("QuotationStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
