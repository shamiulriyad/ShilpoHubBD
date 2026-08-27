namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class DataCollectionEventDto
{
    public Guid Id { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public Guid? SurveyResponseId { get; set; }
    public DateTime CreatedAt { get; set; }
}
