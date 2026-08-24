namespace ShilpoHubBD.Domain.Entities.Learning;

public class CourseModule
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
}
