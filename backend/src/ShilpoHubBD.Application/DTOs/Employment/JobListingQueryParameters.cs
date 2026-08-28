namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobListingQueryParameters
{
    public string? EmploymentType { get; set; }
    public string? Location { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
