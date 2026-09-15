using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Configurations;

public class UnescoRecordConfiguration : IEntityTypeConfiguration<UnescoRecord>
{
    public void Configure(EntityTypeBuilder<UnescoRecord> builder)
    {
        builder.ToTable("UnescoRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(4000);
        builder.Property(r => r.ImageUrl).HasMaxLength(1000);
        builder.Property(r => r.OfficialUrl).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => r.DisplayOrder);

        builder.HasOne(r => r.District)
            .WithMany()
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
