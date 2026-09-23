namespace ShilpoHubBD.Infrastructure.Options;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-flash-lite-latest";
    public int TimeoutSeconds { get; set; } = 30;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
}
