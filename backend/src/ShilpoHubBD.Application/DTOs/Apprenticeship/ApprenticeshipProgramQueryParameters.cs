namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ApprenticeshipProgramQueryParameters
{
    public string? Type { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public string? Location { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
