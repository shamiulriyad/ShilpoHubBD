using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.FieldResearch;

/// <summary>Append-only data-collection history for a survey.</summary>
public class DataCollectionEvent
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;

    public Guid ActorUserId { get; set; }
    public User Actor { get; set; } = null!;

    public DataCollectionEventType EventType { get; set; }
    public string Summary { get; set; } = string.Empty;

    public Guid? SurveyResponseId { get; set; }

    public DateTime CreatedAt { get; set; }
}
