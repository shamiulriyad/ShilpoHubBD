namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class UpdateTrainingMilestoneRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
