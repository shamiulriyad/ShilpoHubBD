namespace ShilpoHubBD.Application.DTOs.Achievement;

public class AwardXpRequest
{
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
