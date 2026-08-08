namespace ShilpoHubBD.Application.DTOs.AIShopping;

public class GiftSuggestionDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal EstimatedPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
}
