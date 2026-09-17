using ShilpoHubBD.Application.DTOs.StoryGenerator;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.StoryGenerator;

public class StoryGeneratorService : IStoryGeneratorService
{
    private readonly IStoryGeneratorProvider _provider;

    public StoryGeneratorService(IStoryGeneratorProvider provider)
    {
        _provider = provider;
    }

    public Task<GeneratedStoryDto> GenerateAsync(GenerateCraftStoryRequest request, CancellationToken cancellationToken)
        => Task.FromResult(_provider.Generate(request));
}
