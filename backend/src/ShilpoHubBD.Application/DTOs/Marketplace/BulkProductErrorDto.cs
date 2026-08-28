namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class BulkProductErrorDto
{
    public int Index { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
