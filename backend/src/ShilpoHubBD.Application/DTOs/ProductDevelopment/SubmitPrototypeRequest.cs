namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class SubmitPrototypeRequest
{
    public string Description { get; set; } = string.Empty;
    public List<PrototypeFileInput> Files { get; set; } = new();
}
