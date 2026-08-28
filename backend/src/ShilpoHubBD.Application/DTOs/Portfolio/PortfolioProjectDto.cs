namespace ShilpoHubBD.Application.DTOs.Portfolio;

public class PortfolioProjectDto
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public string? HeritageSkillName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
