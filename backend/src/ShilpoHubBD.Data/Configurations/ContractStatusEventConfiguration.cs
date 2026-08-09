using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Data.Configurations;

public class ContractStatusEventConfiguration : IEntityTypeConfiguration<ContractStatusEvent>
{
    public void Configure(EntityTypeBuilder<ContractStatusEvent> builder)
    {
        builder.ToTable("ContractStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
