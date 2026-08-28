using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Quotations;

public class QuotationRequestItem
{
    public Guid Id { get; set; }

    public Guid QuotationRequestId { get; set; }
    public QuotationRequest QuotationRequest { get; set; } = null!;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int Quantity { get; set; }
    public decimal? TargetPrice { get; set; }
    public string? Specifications { get; set; }

    public ICollection<QuotationResponseItem> ResponseItems { get; set; } = new List<QuotationResponseItem>();
}
