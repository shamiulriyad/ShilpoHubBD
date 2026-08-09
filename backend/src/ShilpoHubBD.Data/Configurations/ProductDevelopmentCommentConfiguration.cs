using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Configurations;

public class ProductDevelopmentCommentConfiguration : IEntityTypeConfiguration<ProductDevelopmentComment>
{
    public void Configure(EntityTypeBuilder<ProductDevelopmentComment> builder)
    {
        builder.ToTable("ProductDevelopmentComments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
