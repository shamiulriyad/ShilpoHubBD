// Regression checks for the real Translation feature (GeminiTranslationProvider +
// TranslationRequestValidator): Bangla<->English translation via a stubbed Gemini HTTP response,
// empty input and unsupported-language rejection at the validation layer, and that a Gemini
// failure/timeout/missing key raises a clear AiServiceUnavailableException rather than ever
// returning the old fake "[lang] original text" placeholder. No network calls -- a stub
// HttpMessageHandler stands in for Gemini. Run with `dotnet run`.
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Validators.AIShopping;
using ShilpoHubBD.Infrastructure.AIShopping;
using ShilpoHubBD.Infrastructure.Options;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

string GeminiSuccessJson(string translatedText) => JsonSerializer.Serialize(new
{
    candidates = new[]
    {
        new { content = new { parts = new[] { new { text = translatedText } } } },
    },
});

HttpClient MakeClient(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
    new(new StubHandler(responder)) { BaseAddress = new Uri("https://gemini.invalid/v1beta/") };

var options = Options.Create(new GeminiOptions { ApiKey = "test-key", Model = "gemini-test" });

// ===================== Bangla -> English =====================
var bnToEnClient = MakeClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent(GeminiSuccessJson("How are you?"), Encoding.UTF8, "application/json"),
});
var bnToEnProvider = new GeminiTranslationProvider(bnToEnClient, options, NullLogger<GeminiTranslationProvider>.Instance);
var bnToEn = await bnToEnProvider.TranslateAsync(
    new TranslationRequest { Text = "আপনি কেমন আছেন?", TargetLanguage = "English" }, CancellationToken.None);
Check("Bangla to English: translated text comes from the (stubbed) Gemini response", bnToEn.TranslatedText == "How are you?");
Check("Bangla to English: original text is preserved verbatim", bnToEn.OriginalText == "আপনি কেমন আছেন?");
Check("Bangla to English: never returns the old fake '[lang] text' placeholder format", !bnToEn.TranslatedText.StartsWith('['));

// ===================== English -> Bangla =====================
var enToBnClient = MakeClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent(GeminiSuccessJson("আপনি কেমন আছেন?"), Encoding.UTF8, "application/json"),
});
var enToBnProvider = new GeminiTranslationProvider(enToBnClient, options, NullLogger<GeminiTranslationProvider>.Instance);
var enToBn = await enToBnProvider.TranslateAsync(
    new TranslationRequest { Text = "How are you?", TargetLanguage = "Bangla" }, CancellationToken.None);
Check("English to Bangla: translated text comes from the (stubbed) Gemini response", enToBn.TranslatedText == "আপনি কেমন আছেন?");
Check("English to Bangla: never returns the old fake '[lang] text' placeholder format", !enToBn.TranslatedText.StartsWith('['));

// ===================== Empty input =====================
var validator = new TranslationRequestValidator();
var emptyResult = validator.Validate(new TranslationRequest { Text = "", TargetLanguage = "English" });
Check("Empty input is rejected by validation before it can reach the AI provider", !emptyResult.IsValid);

// ===================== Unsupported language =====================
var unsupportedResult = validator.Validate(new TranslationRequest { Text = "Hello", TargetLanguage = "Klingon" });
Check("An unsupported target language is rejected by validation", !unsupportedResult.IsValid);
Check("Bangla (a required supported language) passes validation", validator.Validate(new TranslationRequest { Text = "Hello", TargetLanguage = "Bangla" }).IsValid);
Check("English (a required supported language) passes validation", validator.Validate(new TranslationRequest { Text = "Hello", TargetLanguage = "English" }).IsValid);

// ===================== Gemini/API failure =====================
var failingClient = MakeClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
var failingProvider = new GeminiTranslationProvider(failingClient, options, NullLogger<GeminiTranslationProvider>.Instance);
var threwOnFailure = false;
try
{
    await failingProvider.TranslateAsync(new TranslationRequest { Text = "Hello", TargetLanguage = "Bangla" }, CancellationToken.None);
}
catch (AiServiceUnavailableException)
{
    threwOnFailure = true;
}
Check("A Gemini API failure raises AiServiceUnavailableException, not a fake translation", threwOnFailure);

// ===================== Bonus: timeout =====================
var timeoutClient = MakeClient(_ => throw new TaskCanceledException("simulated timeout"));
var timeoutProvider = new GeminiTranslationProvider(timeoutClient, options, NullLogger<GeminiTranslationProvider>.Instance);
var threwOnTimeout = false;
try
{
    await timeoutProvider.TranslateAsync(new TranslationRequest { Text = "Hello", TargetLanguage = "Bangla" }, CancellationToken.None);
}
catch (AiServiceUnavailableException)
{
    threwOnTimeout = true;
}
Check("A Gemini timeout raises AiServiceUnavailableException, not a fake translation (bonus)", threwOnTimeout);

// ===================== Bonus: missing API key =====================
var noKeyOptions = Options.Create(new GeminiOptions { ApiKey = "", Model = "gemini-test" });
var noKeyProvider = new GeminiTranslationProvider(new HttpClient(), noKeyOptions, NullLogger<GeminiTranslationProvider>.Instance);
var threwOnMissingKey = false;
try
{
    await noKeyProvider.TranslateAsync(new TranslationRequest { Text = "Hello", TargetLanguage = "Bangla" }, CancellationToken.None);
}
catch (AiServiceUnavailableException)
{
    threwOnMissingKey = true;
}
Check("A missing API key raises AiServiceUnavailableException, not a fake translation (bonus)", threwOnMissingKey);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class StubHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
    public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_responder(request));
}
