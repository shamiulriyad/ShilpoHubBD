using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Data.Configurations;

public class DesignCommentConfiguration : IEntityTypeConfiguration<DesignComment>
{
    public void Configure(EntityTypeBuilder<DesignComment> builder)
    {
        builder.ToTable("DesignComments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
