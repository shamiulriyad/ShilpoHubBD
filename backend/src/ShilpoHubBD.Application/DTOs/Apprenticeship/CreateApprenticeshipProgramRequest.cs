namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class CreateApprenticeshipProgramRequest
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? HeritageSkillId { get; set; }
    public string? Location { get; set; }
    public int? DurationWeeks { get; set; }
    public int? Capacity { get; set; }
    public string EligibilityRequirements { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
