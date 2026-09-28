namespace ShilpoHubBD.Infrastructure.Options;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-flash-lite-latest";
    public int TimeoutSeconds { get; set; } = 30;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";

    // Left empty on purpose: an image-generation-capable Gemini model id, verified separately since
    // it needs its own API entitlement/quota distinct from plain text generation. When empty,
    // GeminiInteriorPreviewProvider falls back to the placeholder rather than guessing a model name.
    public string ImageModel { get; set; } = string.Empty;
}
