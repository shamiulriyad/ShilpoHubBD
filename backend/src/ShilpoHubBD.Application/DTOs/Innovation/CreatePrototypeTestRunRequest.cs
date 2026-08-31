using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreatePrototypeTestRunRequest
{
    public Guid? PrototypeIterationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Environment { get; set; }
    public List<TestResultInputDto> Results { get; set; } = new();
}
