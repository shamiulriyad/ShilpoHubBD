namespace ShilpoHubBD.Domain.Entities.Innovation;

public class PrototypeTestCase
{
    public Guid Id { get; set; }

    public Guid InnovationPrototypeId { get; set; }
    public InnovationPrototype Prototype { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Steps { get; set; }
    public string ExpectedResult { get; set; } = string.Empty;

    public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PrototypeTestResult> Results { get; set; } = new List<PrototypeTestResult>();
}
