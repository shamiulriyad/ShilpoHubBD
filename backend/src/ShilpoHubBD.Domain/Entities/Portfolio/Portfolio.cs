using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Portfolio;

public class Portfolio
{
    public Guid Id { get; set; }

    public Guid AcademyMemberProfileId { get; set; }
    public AcademyMemberProfile AcademyMemberProfile { get; set; } = null!;

    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public PortfolioVisibility Visibility { get; set; } = PortfolioVisibility.Private;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PortfolioProject> Projects { get; set; } = new List<PortfolioProject>();
}
