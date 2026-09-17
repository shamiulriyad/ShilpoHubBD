using ShilpoHubBD.Application.DTOs.StoryGenerator;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.StoryGenerator;

/// <summary>Template-based narrative assembly from structured inputs — no external LLM call. Swap in a
/// model-backed <see cref="IStoryGeneratorProvider"/> later if desired.</summary>
public class RuleBasedStoryGeneratorProvider : IStoryGeneratorProvider
{
    public GeneratedStoryDto Generate(GenerateCraftStoryRequest request)
    {
        var place = string.IsNullOrWhiteSpace(request.VillageOrDistrict) ? "Bangladesh" : request.VillageOrDistrict.Trim();
        var materials = request.Materials.Count > 0 ? string.Join(", ", request.Materials) : "locally sourced materials";
        var highlights = request.Highlights.Count > 0
            ? " " + string.Join(" ", request.Highlights.Select(h => h.TrimEnd('.') + "."))
            : string.Empty;

        var title = $"{request.ProductName}: A {request.CraftType} Tradition from {place}";

        var story =
            $"In the heart of {place}, {request.ProducerName} carries forward the art of {request.CraftType}, "
            + $"a craft passed down through generations. Each {request.ProductName} is shaped by hand using "
            + $"{materials}, honouring techniques refined over decades of practice.{highlights} "
            + $"When you bring home this {request.ProductName}, you carry a piece of {place}'s living heritage "
            + "and support the artisan who made it.";

        return new GeneratedStoryDto { Title = title, Story = story };
    }
}
