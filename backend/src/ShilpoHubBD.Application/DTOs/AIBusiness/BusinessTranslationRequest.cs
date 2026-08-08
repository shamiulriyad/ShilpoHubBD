namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class BusinessTranslationRequest
{
    public string Text { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
}
