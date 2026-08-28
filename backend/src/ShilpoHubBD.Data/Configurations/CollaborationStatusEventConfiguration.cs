using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Data.Configurations;

public class CollaborationStatusEventConfiguration : IEntityTypeConfiguration<CollaborationStatusEvent>
{
    public void Configure(EntityTypeBuilder<CollaborationStatusEvent> builder)
    {
        builder.ToTable("CollaborationStatusEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
