namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class DevelopmentMilestoneInput
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
}
