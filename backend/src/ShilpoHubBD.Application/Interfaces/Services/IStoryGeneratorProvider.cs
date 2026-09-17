using ShilpoHubBD.Application.DTOs.StoryGenerator;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IStoryGeneratorProvider
{
    GeneratedStoryDto Generate(GenerateCraftStoryRequest request);
}
