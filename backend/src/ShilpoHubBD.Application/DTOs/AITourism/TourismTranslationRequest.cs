namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourismTranslationRequest
{
    public string Text { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
}
