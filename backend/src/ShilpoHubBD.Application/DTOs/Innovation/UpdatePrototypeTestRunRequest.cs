using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdatePrototypeTestRunRequest
{
    public Guid? PrototypeIterationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Environment { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
    public List<TestResultInputDto> Results { get; set; } = new();
}
