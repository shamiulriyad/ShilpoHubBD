using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Configurations;

public class SavedTourPlanConfiguration : IEntityTypeConfiguration<SavedTourPlan>
{
    public void Configure(EntityTypeBuilder<SavedTourPlan> builder)
    {
        builder.ToTable("SavedTourPlans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.DistrictName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.OriginText).HasMaxLength(200);
        builder.Property(p => p.TransportMode).IsRequired().HasMaxLength(30);
        builder.Property(p => p.TotalEstimatedCost).HasColumnType("decimal(18,2)");
        builder.Property(p => p.RequestJson).IsRequired().HasColumnType("jsonb");
        builder.Property(p => p.PlanJson).IsRequired().HasColumnType("jsonb");
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.UserId, p.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
