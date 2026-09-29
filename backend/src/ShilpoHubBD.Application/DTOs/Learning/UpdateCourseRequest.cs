namespace ShilpoHubBD.Application.DTOs.Learning;

public class UpdateCourseRequest
{
    public decimal Price { get; set; }
    public int DurationDays { get; set; } = 1;
    public int DaysPerWeek { get; set; } = 1;
    public int SessionMinutes { get; set; } = 60;
    public string ClassTime { get; set; } = "10:00";
    public string DeliveryMode { get; set; } = "Online";
    public string? Venue { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public int? MaxApprentices { get; set; }
}
