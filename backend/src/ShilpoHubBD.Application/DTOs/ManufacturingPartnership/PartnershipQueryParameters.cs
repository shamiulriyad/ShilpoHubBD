using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

public class PartnershipQueryParameters
{
    public PartnershipStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
