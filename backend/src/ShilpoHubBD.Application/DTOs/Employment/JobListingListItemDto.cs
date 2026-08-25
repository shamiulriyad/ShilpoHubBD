namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobListingListItemDto
{
    public Guid Id { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public int? MinExperienceYears { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ApplicationCount { get; set; }
    public DateTime? PublishedAt { get; set; }
}
