namespace ShilpoHubBD.Infrastructure.Options;

public class RagServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8000";
    public string Collection { get; set; } = "shilpohub";
    public int TimeoutSeconds { get; set; } = 30;
}
