using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.HeritageAssistant;

/// <summary>Keyword-matching Q&amp;A over the platform's own district/festival/UNESCO data — no external
/// LLM call. Swap in a real model-backed <see cref="IHeritageAssistantProvider"/> later if desired.</summary>
public class RuleBasedHeritageAssistantProvider : IHeritageAssistantProvider
{
    public Task<HeritageAssistantAnswerDto> AnswerAsync(HeritageAssistantContext context, CancellationToken cancellationToken)
        => Task.FromResult(Answer(context));

    private static HeritageAssistantAnswerDto Answer(HeritageAssistantContext context)
    {
        var question = context.Question;

        var matchedDistricts = context.Districts
            .Where(d => question.Contains(d.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var matchedFestivals = context.Festivals
            .Where(f => question.Contains(f.Name, StringComparison.OrdinalIgnoreCase)
                || matchedDistricts.Any(d => d.Name == f.DistrictName)
                || (question.Contains("festival", StringComparison.OrdinalIgnoreCase) && matchedDistricts.Count == 0))
            .Take(5)
            .ToList();

        var matchedUnesco = context.UnescoRecords
            .Where(u => question.Contains(u.Title, StringComparison.OrdinalIgnoreCase)
                || question.Contains("unesco", StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        var answer = BuildAnswer(question, matchedDistricts, matchedFestivals, matchedUnesco);

        return new HeritageAssistantAnswerDto
        {
            Answer = answer,
            RelatedDistricts = matchedDistricts.Select(d => d.Name).ToList(),
            RelatedFestivals = matchedFestivals.Select(f => f.Name).ToList(),
            RelatedUnescoRecords = matchedUnesco.Select(u => u.Title).ToList(),
        };
    }

    private static string BuildAnswer(
        string question,
        List<HeritageDistrictFact> districts,
        List<HeritageFestivalFact> festivals,
        List<HeritageUnescoFact> unesco)
    {
        if (districts.Count == 0 && festivals.Count == 0 && unesco.Count == 0)
        {
            return "I couldn't find anything specific about that in our heritage records yet. "
                + "Try asking about a district, a festival, or a UNESCO-recognised site by name.";
        }

        var parts = new List<string>();

        if (districts.Count > 0)
        {
            var d = districts[0];
            parts.Add($"{d.Name} is a district in {d.Division} division known for its craft heritage.");
        }

        if (festivals.Count > 0)
        {
            var names = string.Join(", ", festivals.Select(f => f.Name));
            parts.Add($"Related festivals: {names}.");
        }

        if (unesco.Count > 0)
        {
            var u = unesco[0];
            parts.Add($"{u.Title} was inscribed in {u.InscribedYear}: {Truncate(u.Description, 200)}");
        }

        return string.Join(" ", parts);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max].TrimEnd() + "…";
}
