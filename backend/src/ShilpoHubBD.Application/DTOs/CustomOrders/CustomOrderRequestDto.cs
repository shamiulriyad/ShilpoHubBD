using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Application.DTOs.CustomOrders;

public class CustomOrderRequestDto
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Specifications { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }

    public CustomOrderStatus Status { get; set; }
    public decimal? QuotedPrice { get; set; }
    public string? ProducerResponse { get; set; }
    public DateTime? RespondedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
