using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchPublicationConfiguration : IEntityTypeConfiguration<ResearchPublication>
{
    public void Configure(EntityTypeBuilder<ResearchPublication> builder)
    {
        builder.ToTable("ResearchPublications");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Authors).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.Venue).HasMaxLength(300);
        builder.Property(p => p.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Doi).HasMaxLength(200);
        builder.Property(p => p.Url).HasMaxLength(2048);
        builder.Property(p => p.Abstract).HasMaxLength(8000);
        builder.Property(p => p.Citation).HasMaxLength(2000);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ResearchProjectId);
        builder.HasIndex(p => p.IsPublic);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.PublishedOn);

        builder.HasOne(p => p.Paper)
            .WithMany(paper => paper.Publications)
            .HasForeignKey(p => p.ResearchPaperId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
