using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Data.Configurations;

public class DesignCollaborationProjectConfiguration : IEntityTypeConfiguration<DesignCollaborationProject>
{
    public void Configure(EntityTypeBuilder<DesignCollaborationProject> builder)
    {
        builder.ToTable("DesignCollaborationProjects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.DesignRequirements).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ReferenceNumber).IsUnique();
        builder.HasIndex(p => p.BusinessPartnerId);
        builder.HasIndex(p => p.ProducerId);

        builder.HasOne(p => p.BusinessPartner)
            .WithMany()
            .HasForeignKey(p => p.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Files)
            .WithOne(f => f.Project)
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Project)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Revisions)
            .WithOne(r => r.Project)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StatusHistory)
            .WithOne(h => h.Project)
            .HasForeignKey(h => h.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
