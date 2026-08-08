namespace ShilpoHubBD.Application.DTOs.Inventory;

public class AdjustStockRequest
{
    public Guid? VariantId { get; set; }
    public int ChangeAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
