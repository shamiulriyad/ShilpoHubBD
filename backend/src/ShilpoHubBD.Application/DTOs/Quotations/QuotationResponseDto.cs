using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationResponseDto
{
    public Guid Id { get; set; }
    public Guid QuotationRequestProducerId { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Notes { get; set; }

    public QuotationResponseStatus Status { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }

    public List<QuotationResponseItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
