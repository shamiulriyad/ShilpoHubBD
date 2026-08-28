namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobMatchResultDto
{
    public Guid JobListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public int? MinExperienceYears { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public decimal MatchScore { get; set; }
    public List<string> MatchReasons { get; set; } = new();
}
