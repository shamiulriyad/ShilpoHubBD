namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchTaskQueryParameters
{
    public string? Status { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? MilestoneId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
