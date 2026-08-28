using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Portfolio;

public class PortfolioProject
{
    public Guid Id { get; set; }

    public Guid PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;

    public Guid? HeritageSkillId { get; set; }
    public HeritageSkill? HeritageSkill { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
