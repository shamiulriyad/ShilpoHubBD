namespace ShilpoHubBD.Application.DTOs.Quotations;

public class CreateQuotationRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }
    public List<Guid> ProducerIds { get; set; } = new();
    public List<QuotationRequestItemInput> Items { get; set; } = new();
}
