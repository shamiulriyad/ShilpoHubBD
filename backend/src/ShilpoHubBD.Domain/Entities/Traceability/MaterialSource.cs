namespace ShilpoHubBD.Domain.Entities.Traceability;

public class MaterialSource
{
    public Guid Id { get; set; }

    public Guid ProductTraceabilityId { get; set; }
    public ProductTraceability ProductTraceability { get; set; } = null!;

    public string MaterialName { get; set; } = string.Empty;
    public string SourceLocation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
