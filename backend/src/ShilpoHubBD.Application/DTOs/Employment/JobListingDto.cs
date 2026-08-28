namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobListingDto
{
    public Guid Id { get; set; }
    public Guid BusinessPartnerProfileId { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public string EmployerIndustry { get; set; } = string.Empty;
    public string? EmployerWebsite { get; set; }
    public string EmployerCity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public int? MinExperienceYears { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<JobSkillRequirementDto> SkillRequirements { get; set; } = new();
    public int ApplicationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
