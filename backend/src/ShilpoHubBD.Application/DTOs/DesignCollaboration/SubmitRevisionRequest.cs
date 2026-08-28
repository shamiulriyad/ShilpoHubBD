namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class SubmitRevisionRequest
{
    public string Description { get; set; } = string.Empty;
    public List<DesignFileInput> Files { get; set; } = new();
}
