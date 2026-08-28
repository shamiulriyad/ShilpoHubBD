namespace ShilpoHubBD.Application.DTOs.Learning;

public class CourseQueryParameters
{
    public string? Category { get; set; }
    public Guid? MentorId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
