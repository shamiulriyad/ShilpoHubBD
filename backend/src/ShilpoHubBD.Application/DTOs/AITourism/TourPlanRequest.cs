namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourPlanRequest
{
    public Guid? DistrictId { get; set; }
    public int DurationDays { get; set; } = 1;
    public int PartySize { get; set; } = 1;
    public DateTime? StartDate { get; set; }
}
