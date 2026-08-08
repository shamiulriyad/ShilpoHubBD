namespace ShilpoHubBD.Application.DTOs.Analytics;

public class FavoriteCategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalSpent { get; set; }
}
