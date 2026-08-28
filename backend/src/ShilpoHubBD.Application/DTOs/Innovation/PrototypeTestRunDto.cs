using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class PrototypeTestRunDto
{
    public Guid Id { get; set; }
    public Guid InnovationPrototypeId { get; set; }
    public Guid? PrototypeIterationId { get; set; }
    public int? IterationVersionNumber { get; set; }
    public int RunNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Environment { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalCases { get; set; }
    public int PassedCases { get; set; }
    public int FailedCases { get; set; }
    public int BlockedCases { get; set; }
    public Guid ExecutedByUserId { get; set; }
    public string ExecutedByName { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrototypeTestResultDto> Results { get; set; } = new();
}
