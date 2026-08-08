namespace ShilpoHubBD.Application.DTOs.Learning;

public class MentorListItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public int PublishedCourseCount { get; set; }
}
