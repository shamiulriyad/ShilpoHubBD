using ShilpoHubBD.Application.DTOs.HeritageAssistant;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over the "intelligence" behind the AI Heritage Assistant. A pure function of a
// pre-fetched context, so a future Gemini/OpenAI implementation can replace
// RuleBasedHeritageAssistantProvider without touching HeritageAssistantService or its controller.
public interface IHeritageAssistantProvider
{
    HeritageAssistantAnswerDto Answer(HeritageAssistantContext context);
}
