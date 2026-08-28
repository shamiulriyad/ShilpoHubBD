namespace ShilpoHubBD.Domain.Entities.Apprenticeship;

public class TrainingMilestone
{
    public Guid Id { get; set; }

    public Guid ProgramId { get; set; }
    public ApprenticeshipProgram Program { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
