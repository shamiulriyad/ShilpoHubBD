using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class DevelopmentStatusEventDto
{
    public DevelopmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
