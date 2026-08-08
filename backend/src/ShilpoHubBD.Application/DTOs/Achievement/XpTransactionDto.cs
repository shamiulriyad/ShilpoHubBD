namespace ShilpoHubBD.Application.DTOs.Achievement;

public class XpTransactionDto
{
    public Guid Id { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
