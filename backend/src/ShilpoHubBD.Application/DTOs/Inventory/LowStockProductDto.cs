namespace ShilpoHubBD.Application.DTOs.Inventory;

public class LowStockProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int LowStockThreshold { get; set; }
    public string? PrimaryImageUrl { get; set; }
}
