namespace ShilpoHubBD.Application.DTOs.Inventory;

public class InventoryTransactionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int ChangeAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
