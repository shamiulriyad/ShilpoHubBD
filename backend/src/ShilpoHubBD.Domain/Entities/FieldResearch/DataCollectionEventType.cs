namespace ShilpoHubBD.Domain.Entities.FieldResearch;

public enum DataCollectionEventType
{
    SurveyCreated,
    SurveyUpdated,
    SurveyStatusChanged,
    QuestionAdded,
    QuestionUpdated,
    QuestionRemoved,
    FieldResearcherAssigned,
    FieldResearcherUpdated,
    FieldResearcherRemoved,
    ResponseCreated,
    ResponseSubmitted,
    ResponseReviewed,
    EvidenceAdded,
    EvidenceRemoved,
}
