using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchNoteConfiguration : IEntityTypeConfiguration<ResearchNote>
{
    public void Configure(EntityTypeBuilder<ResearchNote> builder)
    {
        builder.ToTable("ResearchNotes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Content).IsRequired().HasMaxLength(16000);
        builder.Property(n => n.Visibility).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();

        builder.HasIndex(n => n.ResearchProjectId);
        builder.HasIndex(n => n.AuthorUserId);

        builder.HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
