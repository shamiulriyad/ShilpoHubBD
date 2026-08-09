using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Data.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Terms).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => c.ReferenceNumber).IsUnique();
        builder.HasIndex(c => c.BusinessPartnerId);
        builder.HasIndex(c => c.ProducerId);

        builder.HasOne(c => c.BusinessPartner)
            .WithMany()
            .HasForeignKey(c => c.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Producer)
            .WithMany()
            .HasForeignKey(c => c.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PreviousContract)
            .WithMany()
            .HasForeignKey(c => c.PreviousContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Contract)
            .HasForeignKey(i => i.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.DeliverySchedules)
            .WithOne(s => s.Contract)
            .HasForeignKey(s => s.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documents)
            .WithOne(d => d.Contract)
            .HasForeignKey(d => d.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.StatusHistory)
            .WithOne(h => h.Contract)
            .HasForeignKey(h => h.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
