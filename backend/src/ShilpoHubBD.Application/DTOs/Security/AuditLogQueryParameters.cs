namespace ShilpoHubBD.Application.DTOs.Security;

public class AuditLogQueryParameters
{
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? Search { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
