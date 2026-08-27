using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageRiskRecordConfiguration : IEntityTypeConfiguration<HeritageRiskRecord>
{
    public void Configure(EntityTypeBuilder<HeritageRiskRecord> builder)
    {
        builder.ToTable("HeritageRiskRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(4000);
        builder.Property(r => r.Category).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Level).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.CraftName).HasMaxLength(150);
        builder.Property(r => r.ContributingFactors).HasMaxLength(2000);
        builder.Property(r => r.RecommendedActions).HasMaxLength(2000);
        builder.Property(r => r.Source).HasMaxLength(300);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.DistrictId);
        builder.HasIndex(r => r.VillageId);
        builder.HasIndex(r => r.ProducerId);
        builder.HasIndex(r => r.Level);
        builder.HasIndex(r => r.Category);
        builder.HasIndex(r => r.AssessmentYear);

        builder.HasOne(r => r.District)
            .WithMany()
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Village)
            .WithMany()
            .HasForeignKey(r => r.VillageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Producer)
            .WithMany()
            .HasForeignKey(r => r.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
