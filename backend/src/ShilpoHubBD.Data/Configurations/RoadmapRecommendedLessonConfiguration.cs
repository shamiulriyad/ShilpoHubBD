using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Data.Configurations;

public class RoadmapRecommendedLessonConfiguration : IEntityTypeConfiguration<RoadmapRecommendedLesson>
{
    public void Configure(EntityTypeBuilder<RoadmapRecommendedLesson> builder)
    {
        builder.ToTable("RoadmapRecommendedLessons");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Reason).IsRequired().HasMaxLength(1000);

        builder.HasIndex(l => l.RoadmapMilestoneId);
        builder.HasIndex(l => l.CourseLessonId);

        builder.HasOne(l => l.CourseLesson)
            .WithMany()
            .HasForeignKey(l => l.CourseLessonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
