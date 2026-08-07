using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Configurations;

public class CommunityAnswerConfiguration : IEntityTypeConfiguration<CommunityAnswer>
{
    public void Configure(EntityTypeBuilder<CommunityAnswer> builder)
    {
        builder.ToTable("CommunityAnswers");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Body).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.QuestionId);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
