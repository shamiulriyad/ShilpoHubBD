namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class TrainingMilestoneDto
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
