using ShilpoHubBD.Application.DTOs.HeritageAssistant;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageAssistantService
{
    Task<HeritageAssistantAnswerDto> AskAsync(AskHeritageAssistantRequest request, CancellationToken cancellationToken);
}
