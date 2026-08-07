namespace ShilpoHubBD.Application.DTOs.Commerce;

public class OrderStatusEventDto
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
