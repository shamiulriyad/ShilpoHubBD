using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerOrderItemQueryParameters
{
    public OrderItemProducerStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
