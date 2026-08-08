using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.CustomOrders;

public class CustomOrderRequest
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    // Optional reference product this custom request is based on/inspired by.
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Specifications { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }

    public CustomOrderStatus Status { get; set; } = CustomOrderStatus.Pending;
    public decimal? QuotedPrice { get; set; }
    public string? ProducerResponse { get; set; }
    public DateTime? RespondedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
