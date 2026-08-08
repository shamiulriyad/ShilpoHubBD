namespace ShilpoHubBD.Application.DTOs.Traceability;

public class MaterialSourceDto
{
    public string MaterialName { get; set; } = string.Empty;
    public string SourceLocation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
