namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int ItemsSold { get; set; }
}
