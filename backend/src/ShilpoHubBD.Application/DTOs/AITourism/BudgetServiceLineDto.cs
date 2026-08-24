namespace ShilpoHubBD.Application.DTOs.AITourism;

public class BudgetServiceLineDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int PartySize { get; set; }
}
