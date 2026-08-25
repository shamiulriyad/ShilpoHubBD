using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Data.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolios");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Headline).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Summary).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Visibility).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.AcademyMemberProfileId).IsUnique();

        builder.HasOne(p => p.AcademyMemberProfile)
            .WithMany()
            .HasForeignKey(p => p.AcademyMemberProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Projects)
            .WithOne(pr => pr.Portfolio)
            .HasForeignKey(pr => pr.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
