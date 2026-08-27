using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Data.Configurations;

public class KnowledgeNodeConfiguration : IEntityTypeConfiguration<KnowledgeNode>
{
    public void Configure(EntityTypeBuilder<KnowledgeNode> builder)
    {
        builder.ToTable("KnowledgeNodes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.NodeType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Label).IsRequired().HasMaxLength(200);
        builder.Property(n => n.LabelNormalized).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Description).HasMaxLength(2000);
        builder.Property(n => n.MetadataJson).HasMaxLength(8000);
        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();

        builder.HasIndex(n => n.NodeType);
        builder.HasIndex(n => new { n.NodeType, n.LabelNormalized }).IsUnique();
        builder.HasIndex(n => new { n.NodeType, n.ExternalEntityId })
            .IsUnique()
            .HasFilter("\"ExternalEntityId\" IS NOT NULL");

        builder.HasOne(n => n.CreatedBy)
            .WithMany()
            .HasForeignKey(n => n.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(n => n.OutgoingRelationships)
            .WithOne(r => r.SourceNode)
            .HasForeignKey(r => r.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.IncomingRelationships)
            .WithOne(r => r.TargetNode)
            .HasForeignKey(r => r.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
