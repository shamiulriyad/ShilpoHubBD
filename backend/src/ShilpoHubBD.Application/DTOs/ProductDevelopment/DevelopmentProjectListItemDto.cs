using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class DevelopmentProjectListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public DevelopmentStatus Status { get; set; }
    public int PrototypeVersionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
