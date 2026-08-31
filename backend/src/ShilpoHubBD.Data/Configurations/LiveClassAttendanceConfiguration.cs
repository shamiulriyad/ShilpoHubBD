using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Data.Configurations;

public class LiveClassAttendanceConfiguration : IEntityTypeConfiguration<LiveClassAttendance>
{
    public void Configure(EntityTypeBuilder<LiveClassAttendance> builder)
    {
        builder.ToTable("LiveClassAttendances");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.JoinedAt).IsRequired();

        builder.HasIndex(a => new { a.LiveClassId, a.UserId });

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
