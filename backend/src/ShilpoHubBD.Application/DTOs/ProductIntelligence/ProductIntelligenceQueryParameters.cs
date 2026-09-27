namespace ShilpoHubBD.Application.DTOs.ProductIntelligence;

public enum ProductIntelligenceRange
{
    Last30Days,
    Last3Months,
    Last5Months,
    Last12Months,
}

public class ProductIntelligenceQueryParameters
{
    public ProductIntelligenceRange Range { get; set; } = ProductIntelligenceRange.Last30Days;
}
