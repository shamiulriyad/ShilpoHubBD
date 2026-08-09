using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class PrototypeVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public PrototypeStatus Status { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public List<PrototypeFileDto> Files { get; set; } = new();
}
