using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Assessment;

public class Assignment
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public DateTime? DueAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}
