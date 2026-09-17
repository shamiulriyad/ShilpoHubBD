using ShilpoHubBD.Application.DTOs.StoryGenerator;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IStoryGeneratorService
{
    Task<GeneratedStoryDto> GenerateAsync(GenerateCraftStoryRequest request, CancellationToken cancellationToken);
}
