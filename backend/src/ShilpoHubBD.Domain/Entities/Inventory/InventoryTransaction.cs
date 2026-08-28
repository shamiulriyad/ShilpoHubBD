using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Inventory;

public class InventoryTransaction
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public int ChangeAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
