using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Data.Configurations;

public class RoadmapRecommendedCourseConfiguration : IEntityTypeConfiguration<RoadmapRecommendedCourse>
{
    public void Configure(EntityTypeBuilder<RoadmapRecommendedCourse> builder)
    {
        builder.ToTable("RoadmapRecommendedCourses");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Reason).IsRequired().HasMaxLength(1000);

        builder.HasIndex(c => c.RoadmapMilestoneId);
        builder.HasIndex(c => c.CourseId);

        builder.HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
