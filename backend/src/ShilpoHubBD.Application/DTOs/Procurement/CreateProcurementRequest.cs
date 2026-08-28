namespace ShilpoHubBD.Application.DTOs.Procurement;

public class CreateProcurementRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public decimal? Budget { get; set; }
    public DateTime DeliveryDeadline { get; set; }
    public List<ProcurementItemInput> Items { get; set; } = new();
}
