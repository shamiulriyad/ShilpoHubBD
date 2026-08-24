using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Data.Configurations;

public class LiveClassConfiguration : IEntityTypeConfiguration<LiveClass>
{
    public void Configure(EntityTypeBuilder<LiveClass> builder)
    {
        builder.ToTable("LiveClasses");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.MeetingUrl).HasMaxLength(2000);
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ScheduledStartAt).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.ScheduledStartAt);
        builder.HasIndex(c => c.InstructorUserId);

        builder.HasOne(c => c.Instructor)
            .WithMany()
            .HasForeignKey(c => c.InstructorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.LiveClass)
            .HasForeignKey(p => p.LiveClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Questions)
            .WithOne(q => q.LiveClass)
            .HasForeignKey(q => q.LiveClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Attendances)
            .WithOne(a => a.LiveClass)
            .HasForeignKey(a => a.LiveClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
