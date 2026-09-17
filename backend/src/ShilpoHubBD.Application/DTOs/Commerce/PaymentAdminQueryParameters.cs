namespace ShilpoHubBD.Application.DTOs.Commerce;

/// <summary>Filters for the Super Admin "Refund Management" payments queue.</summary>
public class PaymentAdminQueryParameters
{
    /// <summary>A <see cref="ShilpoHubBD.Domain.Entities.Commerce.PaymentStatus"/> name, e.g. "Refunded".</summary>
    public string? Status { get; set; }
    public string? Search { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
