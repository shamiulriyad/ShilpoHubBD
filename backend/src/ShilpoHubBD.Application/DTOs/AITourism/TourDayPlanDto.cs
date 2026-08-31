namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourDayPlanDto
{
    public int DayNumber { get; set; }
    public DateTime? Date { get; set; }
    public List<TourStopDto> Stops { get; set; } = new();
}
