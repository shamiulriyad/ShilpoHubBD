using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchPaperConfiguration : IEntityTypeConfiguration<ResearchPaper>
{
    public void Configure(EntityTypeBuilder<ResearchPaper> builder)
    {
        builder.ToTable("ResearchPapers");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Abstract).HasMaxLength(8000);
        builder.Property(p => p.Authors).HasMaxLength(2000);
        builder.Property(p => p.Keywords).HasMaxLength(1000);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ManuscriptUrl).HasMaxLength(2048);
        builder.Property(p => p.TargetVenue).HasMaxLength(300);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ResearchProjectId);
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
