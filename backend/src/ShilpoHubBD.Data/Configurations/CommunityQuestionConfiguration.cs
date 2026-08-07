using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Configurations;

public class CommunityQuestionConfiguration : IEntityTypeConfiguration<CommunityQuestion>
{
    public void Configure(EntityTypeBuilder<CommunityQuestion> builder)
    {
        builder.ToTable("CommunityQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Body).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.CreatedAt).IsRequired();

        builder.HasIndex(q => q.ProductId);

        builder.HasOne(q => q.Product)
            .WithMany()
            .HasForeignKey(q => q.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
