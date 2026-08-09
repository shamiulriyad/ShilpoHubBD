using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Data.Configurations;

public class InvestmentStatusEventConfiguration : IEntityTypeConfiguration<InvestmentStatusEvent>
{
    public void Configure(EntityTypeBuilder<InvestmentStatusEvent> builder)
    {
        builder.ToTable("InvestmentStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
