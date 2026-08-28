namespace ShilpoHubBD.Application.DTOs.Portfolio;

public class UpdatePortfolioProjectRequest
{
    public Guid? HeritageSkillId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}
