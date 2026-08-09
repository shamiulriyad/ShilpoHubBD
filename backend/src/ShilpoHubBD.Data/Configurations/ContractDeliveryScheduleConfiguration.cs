using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Data.Configurations;

public class ContractDeliveryScheduleConfiguration : IEntityTypeConfiguration<ContractDeliverySchedule>
{
    public void Configure(EntityTypeBuilder<ContractDeliverySchedule> builder)
    {
        builder.ToTable("ContractDeliverySchedules");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(1000);
    }
}
