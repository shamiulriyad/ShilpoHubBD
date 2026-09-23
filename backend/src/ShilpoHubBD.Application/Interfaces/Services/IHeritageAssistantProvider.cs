using ShilpoHubBD.Application.DTOs.HeritageAssistant;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over the "intelligence" behind the AI Heritage Assistant. A pure function of a
// pre-fetched context, so a future Gemini/OpenAI implementation can replace
// RuleBasedHeritageAssistantProvider without touching HeritageAssistantService or its controller.
// Async because the real implementation (RagHeritageAssistantProvider) calls out to the RAG
// service over HTTP.
public interface IHeritageAssistantProvider
{
    Task<HeritageAssistantAnswerDto> AnswerAsync(HeritageAssistantContext context, CancellationToken cancellationToken);
}
