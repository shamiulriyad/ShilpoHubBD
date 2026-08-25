using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Data.Configurations;

public class PortfolioProjectConfiguration : IEntityTypeConfiguration<PortfolioProject>
{
    public void Configure(EntityTypeBuilder<PortfolioProject> builder)
    {
        builder.ToTable("PortfolioProjects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.ImageUrl).HasMaxLength(1000);
        builder.Property(p => p.ProjectUrl).HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => p.PortfolioId);

        builder.HasOne(p => p.HeritageSkill)
            .WithMany()
            .HasForeignKey(p => p.HeritageSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
