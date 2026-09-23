using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Retrieval from the isolated Travel Planner RAG Knowledge Base (a separate Qdrant collection
// from the general Heritage Assistant's -- see rag/travel/). Returns an empty list on any
// failure or if the collection isn't indexed yet -- never throws, never blocks a plan.
public interface ITravelPlannerRagProvider
{
    Task<List<RagTravelNoteDto>> RetrieveAsync(
        string query, string? district, IReadOnlyList<string>? interests, CancellationToken cancellationToken);
}
