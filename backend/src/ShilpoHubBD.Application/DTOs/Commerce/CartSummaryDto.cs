namespace ShilpoHubBD.Application.DTOs.Commerce;

public class CartSummaryDto
{
    public int ItemCount { get; set; }
    public int TotalQuantity { get; set; }
    public decimal Subtotal { get; set; }
}
