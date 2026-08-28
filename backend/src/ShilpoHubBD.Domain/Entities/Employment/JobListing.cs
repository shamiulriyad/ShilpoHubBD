using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Domain.Entities.Employment;

public class JobListing
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerProfileId { get; set; }
    public BusinessPartnerProfile BusinessPartnerProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public int? MinExperienceYears { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    public JobListingStatus Status { get; set; } = JobListingStatus.Draft;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<JobSkillRequirement> SkillRequirements { get; set; } = new List<JobSkillRequirement>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
