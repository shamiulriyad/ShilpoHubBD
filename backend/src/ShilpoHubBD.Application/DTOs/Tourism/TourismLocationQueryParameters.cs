using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.DTOs.Tourism;

public class TourismLocationQueryParameters
{
    public string? Search { get; set; }
    public TourismLocationType? Type { get; set; }
    public Guid? DistrictId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
