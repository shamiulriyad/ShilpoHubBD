using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.AITourism;

/// <summary>
/// Real itinerary narration via the Gemini API. Only <see cref="PlanTourAsync"/> and
/// <see cref="TranslateAsync"/> actually call the model -- day-plan phrasing and translation are
/// genuine language tasks. Budget math, route ordering and recommendation scoring stay on the
/// deterministic <see cref="DummyAITourismProvider"/> heuristics (delegated to directly), since
/// those need to be exact and verifiable, not generated. Gemini is only ever given the real
/// places/festivals/services already fetched by AITourismService -- it is instructed to organise
/// that data, not invent new attractions, prices or schedules. Any missing API key, HTTP failure,
/// timeout or malformed response falls back to the rule-based provider so the feature never breaks.
/// </summary>
public class GeminiAITourismProvider : IAITourismProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly DummyAITourismProvider _fallback;
    private readonly ILogger<GeminiAITourismProvider> _logger;

    public GeminiAITourismProvider(
        HttpClient httpClient, IOptions<GeminiOptions> options, DummyAITourismProvider fallback, ILogger<GeminiAITourismProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<TourPlanResult> PlanTourAsync(TourPlanContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return await _fallback.PlanTourAsync(context, cancellationToken);
        }

        try
        {
            var prompt = BuildTourPlanPrompt(context);
            var raw = await CallGeminiAsync(prompt, cancellationToken);
            var parsed = JsonSerializer.Deserialize<GeminiTourPlan>(raw, JsonOptions);
            if (parsed is null)
            {
                return await _fallback.PlanTourAsync(context, cancellationToken);
            }

            return GroundResult(parsed, context);
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exc, "Gemini tour-plan call failed; falling back to the rule-based planner.");
            return await _fallback.PlanTourAsync(context, cancellationToken);
        }
    }

    public Task<BudgetPlanResult> PlanBudgetAsync(BudgetPlanContext context, CancellationToken cancellationToken)
        => _fallback.PlanBudgetAsync(context, cancellationToken);

    public Task<RouteOptimizationResult> OptimizeRouteAsync(RouteOptimizationContext context, CancellationToken cancellationToken)
        => _fallback.OptimizeRouteAsync(context, cancellationToken);

    public Task<CulturalRecommendationResult> RecommendAsync(CulturalRecommendationContext context, CancellationToken cancellationToken)
        => _fallback.RecommendAsync(context, cancellationToken);

    public async Task<TourismTranslationResult> TranslateAsync(TourismTranslationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(request.Text))
        {
            return await _fallback.TranslateAsync(request, cancellationToken);
        }

        try
        {
            var prompt = $"Translate the following text to {request.TargetLanguage}. " +
                "Reply with only the translated text, no explanation or quotes.\n\n" + request.Text;
            var raw = await CallGeminiAsync(prompt, cancellationToken, plainText: true);

            return new TourismTranslationResult
            {
                OriginalText = request.Text,
                TranslatedText = raw.Trim(),
                TargetLanguage = request.TargetLanguage,
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exc, "Gemini translate call failed; falling back to the placeholder translator.");
            return await _fallback.TranslateAsync(request, cancellationToken);
        }
    }

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken cancellationToken, bool plainText = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"models/{_options.Model}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(new GeminiRequest
            {
                Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
                GenerationConfig = plainText ? null : new GeminiGenerationConfig { ResponseMimeType = "application/json" },
            }), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
        var text = body?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new JsonException("Gemini response contained no text.");
        }

        return text;
    }

    // Drops any day/stop that references a place or service id Gemini was not actually given,
    // so a hallucinated reference can never surface as if it were a real, bookable attraction.
    private static TourPlanResult GroundResult(GeminiTourPlan parsed, TourPlanContext context)
    {
        var knownPlaceIds = context.Places.Select(p => p.Id).ToHashSet();
        var knownServiceIds = context.Services.Select(s => s.Id).ToHashSet();
        var knownLocationIds = context.TourismLocations.Select(l => l.Id).ToHashSet();

        var days = (parsed.Days ?? [])
            .Select(d => new TourDayPlanDto
            {
                DayNumber = d.DayNumber,
                Date = context.StartDate?.AddDays(d.DayNumber - 1),
                Stops = (d.Stops ?? [])
                    .Select(s =>
                    {
                        var referenceId = s.ReferenceId;
                        var isKnown = referenceId.HasValue
                            && (knownPlaceIds.Contains(referenceId.Value) || knownServiceIds.Contains(referenceId.Value)
                                || knownLocationIds.Contains(referenceId.Value));
                        return new TourStopDto
                        {
                            ReferenceId = isKnown ? referenceId : null,
                            Type = s.Type ?? "FreeTime",
                            Name = s.Name ?? "Free time / local exploration",
                            Notes = s.Notes,
                            EstimatedDurationHours = s.EstimatedDurationHours is > 0 and <= 24 ? s.EstimatedDurationHours : null,
                            DurationIsEstimated = s.DurationIsEstimated ?? true,
                        };
                    })
                    .ToList(),
            })
            .ToList();

        return new TourPlanResult
        {
            Days = days,
            HighlightedFestivals = parsed.HighlightedFestivals ?? [],
            Summary = parsed.Summary ?? string.Empty,
            AccommodationRecommendation = parsed.AccommodationRecommendation,
            UnverifiedNotes = parsed.UnverifiedNotes ?? [],
            IsAiGenerated = true,
        };
    }

    private static string BuildTourPlanPrompt(TourPlanContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a travel itinerary organiser for a Bangladesh heritage tourism platform.");
        sb.AppendLine("Prefer the curated places and services listed below -- use their exact id so the app can show real details. " +
            "If the curated list for this district is thin, you may ALSO suggest a few real, well-known, independently " +
            "verifiable public attractions (for example a named beach, park or landmark you are confident actually exists " +
            "in or near this district) as stops of type \"Attraction\" with no id. Never invent a place, festival, price, " +
            "opening hour or schedule that isn't real.");
        sb.AppendLine("HARD RULES: (1) Never state or guess a transport schedule, ticket price, entry fee, opening hours or hotel " +
            "availability. If the data below does not give it, write exactly \"not verified\" for that item and add a line about it " +
            "to unverifiedNotes. (2) Use the reference notes below as the primary source for what each place is, what to do there, " +
            "and how long to spend (\"Suggested visit duration\"); mention a caution from the notes when one applies. " +
            "(3) Order each day's stops so nearby places follow each other; do not compute distances or road routes -- the app does that. " +
            "(4) Give every visit stop estimatedDurationHours; set durationIsEstimated=false only when a note states the duration, otherwise true. " +
            "(5) Include a lunch stop (type \"Meal\") and, on long days, a \"Rest\" stop, without naming a specific restaurant unless it is in the curated list.");
        sb.AppendLine();
        sb.AppendLine($"Destination district: {context.DistrictName}");
        if (!string.IsNullOrWhiteSpace(context.OriginText)) sb.AppendLine($"Starting from: {context.OriginText}");
        sb.AppendLine($"Duration: {context.DurationDays} day(s)");
        sb.AppendLine($"Party size: {context.PartySize}");
        sb.AppendLine($"Transport mode: {context.TransportMode}");
        if (context.Budget.HasValue) sb.AppendLine($"Traveler's stated budget: BDT {context.Budget.Value:N0}");
        if (context.Preferences.Count > 0) sb.AppendLine($"Interests: {string.Join(", ", context.Preferences)}");
        if (context.TransportEstimate is not null) sb.AppendLine($"Transport note: {context.TransportEstimate.Notes}");

        sb.AppendLine();
        sb.AppendLine("Available heritage places (use their exact id and name):");
        foreach (var place in context.Places)
        {
            sb.AppendLine($"- id={place.Id} name=\"{place.Name}\" type={place.PlaceType} featured={place.IsFeatured}");
        }

        sb.AppendLine("Available bookable services (use their exact id and name):");
        foreach (var service in context.Services)
        {
            sb.AppendLine($"- id={service.Id} name=\"{service.Title}\" type={service.Type} price=BDT{service.Price:N0}");
        }

        sb.AppendLine("Admin-curated tourism locations -- hotels, resorts, restaurants and attractions with real " +
            "prices/coordinates (use their exact id and name; strongly prefer these over a general-knowledge \"Attraction\" " +
            "suggestion when one of these covers the same need):");
        foreach (var location in context.TourismLocations)
        {
            var priceText = location.Price.HasValue ? $"price=BDT{location.Price:N0}/night"
                : location.EntryFee.HasValue ? $"entryFee=BDT{location.EntryFee:N0}" : "price=unknown";
            var hoursText = string.IsNullOrWhiteSpace(location.OpeningHours) ? "openingHours=not verified" : $"openingHours=\"{location.OpeningHours}\"";
            if (!location.Price.HasValue && !location.EntryFee.HasValue) priceText = "price/fee=not verified";
            else if (location.Price.HasValue) priceText += "(lowest listed rate)";
            sb.AppendLine($"- id={location.Id} name=\"{location.Name}\" type={location.Type} area=\"{location.Area}\" {priceText} {hoursText} " +
                $"source={location.VerificationStatus} coordinates={location.CoordinatesPrecision ?? "unknown"}");
        }

        sb.AppendLine("Reference notes retrieved from the Travel Planner knowledge base (verified dataset text; treat as the source of truth about these places):");
        foreach (var note in context.RagNotes.Take(8))
        {
            var text = note.Text.Length > 700 ? note.Text[..700] : note.Text;
            sb.AppendLine($"- {text.ReplaceLineEndings(" ")}");
        }

        sb.AppendLine("Festivals happening in range (mention only if genuinely relevant):");
        foreach (var festival in context.Festivals)
        {
            sb.AppendLine($"- \"{festival.Name}\" ({festival.StartDate:MMM d}-{festival.EndDate:MMM d})");
        }

        sb.AppendLine();
        sb.AppendLine("Respond with strict JSON only, matching this shape: " +
            "{ \"summary\": string, \"accommodationRecommendation\": string, \"unverifiedNotes\": string[], \"highlightedFestivals\": string[], " +
            "\"days\": [ { \"dayNumber\": number, \"stops\": [ { \"referenceId\": string|null, \"type\": " +
            "\"HeritagePlace\"|\"TouristService\"|\"TourismLocation\"|\"Attraction\"|\"Meal\"|\"Rest\"|\"FreeTime\", \"name\": string, \"notes\": string|null (activities to do there), " +
            "\"estimatedDurationHours\": number, \"durationIsEstimated\": boolean } ] } ]. " +
            "accommodationRecommendation must name a curated hotel/resort from the list above if one exists, otherwise say no verified accommodation data is available and suggest checking locally. " +
            "Produce exactly the requested number of days. " +
            "Every stop with type HeritagePlace, TouristService or TourismLocation must use one of the matching ids listed above as referenceId. " +
            "A stop with type Attraction must leave referenceId null and use only a real, well-known place name.");

        return sb.ToString();
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("responseMimeType")]
        public string ResponseMimeType { get; set; } = "application/json";
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private class GeminiTourPlan
    {
        public string? Summary { get; set; }
        public List<string>? HighlightedFestivals { get; set; }
        public string? AccommodationRecommendation { get; set; }
        public List<string>? UnverifiedNotes { get; set; }
        public List<GeminiDayPlan>? Days { get; set; }
    }

    private class GeminiDayPlan
    {
        public int DayNumber { get; set; }
        public List<GeminiStop>? Stops { get; set; }
    }

    private class GeminiStop
    {
        public Guid? ReferenceId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
        public double? EstimatedDurationHours { get; set; }
        public bool? DurationIsEstimated { get; set; }
    }
}
