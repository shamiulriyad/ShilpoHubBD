using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchActivityConfiguration : IEntityTypeConfiguration<ResearchActivity>
{
    public void Configure(EntityTypeBuilder<ResearchActivity> builder)
    {
        builder.ToTable("ResearchActivities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Summary).IsRequired().HasMaxLength(500);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => new { a.ResearchProjectId, a.CreatedAt });

        builder.HasOne(a => a.Actor)
            .WithMany()
            .HasForeignKey(a => a.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
