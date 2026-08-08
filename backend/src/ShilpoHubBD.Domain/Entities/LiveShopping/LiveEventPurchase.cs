using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.LiveShopping;

public class LiveEventPurchase
{
    public Guid Id { get; set; }

    public Guid LiveEventId { get; set; }
    public LiveEvent LiveEvent { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }
}
