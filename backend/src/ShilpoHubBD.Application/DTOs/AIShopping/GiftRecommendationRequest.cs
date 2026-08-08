namespace ShilpoHubBD.Application.DTOs.AIShopping;

public class GiftRecommendationRequest
{
    public string Occasion { get; set; } = string.Empty;
    public string RecipientInterest { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
}
