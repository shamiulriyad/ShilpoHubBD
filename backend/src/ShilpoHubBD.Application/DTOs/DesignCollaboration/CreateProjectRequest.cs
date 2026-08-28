namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class CreateProjectRequest
{
    public Guid ProducerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DesignRequirements { get; set; } = string.Empty;
    public List<DesignFileInput> InitialFiles { get; set; } = new();
}
