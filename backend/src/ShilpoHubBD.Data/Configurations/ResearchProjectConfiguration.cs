using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchProjectConfiguration : IEntityTypeConfiguration<ResearchProject>
{
    public void Configure(EntityTypeBuilder<ResearchProject> builder)
    {
        builder.ToTable("ResearchProjects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Slug).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Summary).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Description).HasMaxLength(8000);
        builder.Property(p => p.Discipline).HasMaxLength(150);
        builder.Property(p => p.Institution).HasMaxLength(200);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Visibility).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.OwnerUserId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Visibility);

        builder.HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Members)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Notes)
            .WithOne(n => n.Project)
            .HasForeignKey(n => n.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Papers)
            .WithOne(paper => paper.Project)
            .HasForeignKey(paper => paper.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Publications)
            .WithOne(pub => pub.Project)
            .HasForeignKey(pub => pub.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Activities)
            .WithOne(a => a.Project)
            .HasForeignKey(a => a.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
