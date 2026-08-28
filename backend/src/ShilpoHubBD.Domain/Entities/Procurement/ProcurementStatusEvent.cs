namespace ShilpoHubBD.Domain.Entities.Procurement;

public class ProcurementStatusEvent
{
    public Guid Id { get; set; }

    public Guid ProcurementRequestId { get; set; }
    public ProcurementRequest ProcurementRequest { get; set; } = null!;

    public ProcurementStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
