using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Configurations;

public class CourseLessonConfiguration : IEntityTypeConfiguration<CourseLesson>
{
    public void Configure(EntityTypeBuilder<CourseLesson> builder)
    {
        builder.ToTable("CourseLessons");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Content).IsRequired().HasMaxLength(8000);
        builder.Property(l => l.VideoUrl).HasMaxLength(2000);
        builder.Property(l => l.DisplayOrder).IsRequired();
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt).IsRequired();

        builder.HasIndex(l => new { l.CourseId, l.DisplayOrder });
    }
}
