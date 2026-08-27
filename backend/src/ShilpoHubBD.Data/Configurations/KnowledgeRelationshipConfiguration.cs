using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Data.Configurations;

public class KnowledgeRelationshipConfiguration : IEntityTypeConfiguration<KnowledgeRelationship>
{
    public void Configure(EntityTypeBuilder<KnowledgeRelationship> builder)
    {
        builder.ToTable("KnowledgeRelationships");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RelationshipType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Label).HasMaxLength(200);
        builder.Property(r => r.Note).HasMaxLength(2000);
        builder.Property(r => r.MetadataJson).HasMaxLength(8000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.RelationshipType);
        builder.HasIndex(r => r.SourceNodeId);
        builder.HasIndex(r => r.TargetNodeId);
        builder.HasIndex(r => new { r.SourceNodeId, r.TargetNodeId, r.RelationshipType }).IsUnique();

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
