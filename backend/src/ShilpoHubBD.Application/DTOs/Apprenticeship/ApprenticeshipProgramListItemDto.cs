namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ApprenticeshipProgramListItemDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int? DurationWeeks { get; set; }
    public int? Capacity { get; set; }
    public string? HeritageSkillName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ActiveEnrollmentCount { get; set; }
    public DateTime? PublishedAt { get; set; }
}
