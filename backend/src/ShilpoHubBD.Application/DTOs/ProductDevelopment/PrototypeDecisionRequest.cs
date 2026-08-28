using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class PrototypeDecisionRequest
{
    public PrototypeStatus Status { get; set; }
    public string? DecisionNotes { get; set; }
}
