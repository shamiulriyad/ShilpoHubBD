using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.DTOs.Commerce;

public class OrderQueryParameters
{
    public OrderStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
