namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class RevenueDashboardDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal AverageOrderValue { get; set; }

    public int PendingCount { get; set; }
    public int AcceptedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
}
