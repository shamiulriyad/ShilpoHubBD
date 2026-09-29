namespace ShilpoHubBD.Application.DTOs.Learning;

public class CourseListItemDto
{
    public decimal Price { get; set; }
    public int DurationDays { get; set; } = 1;
    public int DaysPerWeek { get; set; } = 1;
    public int SessionMinutes { get; set; } = 60;
    public string ClassTime { get; set; } = "10:00";
    public string DeliveryMode { get; set; } = "Online";
    public string? Venue { get; set; }
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int LessonCount { get; set; }
    public int? MaxApprentices { get; set; }
    public int ActiveEnrollmentCount { get; set; }
}
