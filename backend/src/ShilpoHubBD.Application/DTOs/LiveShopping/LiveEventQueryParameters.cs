using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Application.DTOs.LiveShopping;

public class LiveEventQueryParameters
{
    public LiveEventStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
