using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Configurations;

public class ProductDevelopmentProjectConfiguration : IEntityTypeConfiguration<ProductDevelopmentProject>
{
    public void Configure(EntityTypeBuilder<ProductDevelopmentProject> builder)
    {
        builder.ToTable("ProductDevelopmentProjects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BusinessRequirements).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.ProductSpecifications).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.ReferenceNumber).IsUnique();
        builder.HasIndex(p => p.BusinessPartnerId);
        builder.HasIndex(p => p.ProducerId);
        builder.HasIndex(p => p.FinalProductId).IsUnique();

        builder.HasOne(p => p.BusinessPartner)
            .WithMany()
            .HasForeignKey(p => p.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.FinalProduct)
            .WithMany()
            .HasForeignKey(p => p.FinalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PrototypeVersions)
            .WithOne(v => v.Project)
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Project)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StatusHistory)
            .WithOne(h => h.Project)
            .HasForeignKey(h => h.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
