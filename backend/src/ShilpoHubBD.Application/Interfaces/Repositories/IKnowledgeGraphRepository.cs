using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IKnowledgeGraphRepository
{
    // Nodes
    Task<KnowledgeNode?> GetNodeByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<KnowledgeNode>> GetNodesByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<KnowledgeNode?> GetNodeByExternalAsync(KnowledgeNodeType type, Guid externalEntityId, CancellationToken cancellationToken);
    Task<KnowledgeNode?> GetNodeByLabelAsync(KnowledgeNodeType type, string labelNormalized, CancellationToken cancellationToken);
    Task<(List<KnowledgeNode> Items, int TotalCount)> GetNodesPagedAsync(
        KnowledgeNodeQueryParameters query, CancellationToken cancellationToken);
    Task<int> CountRelationshipsForNodeAsync(Guid nodeId, CancellationToken cancellationToken);
    Task AddNodeAsync(KnowledgeNode node, CancellationToken cancellationToken);
    void RemoveNode(KnowledgeNode node);

    // Relationships
    Task<KnowledgeRelationship?> GetRelationshipByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<KnowledgeRelationship?> GetRelationshipAsync(
        Guid sourceNodeId, Guid targetNodeId, KnowledgeRelationshipType type, CancellationToken cancellationToken);
    Task<(List<KnowledgeRelationship> Items, int TotalCount)> GetRelationshipsPagedAsync(
        KnowledgeRelationshipQueryParameters query, CancellationToken cancellationToken);
    Task<List<KnowledgeRelationship>> GetRelationshipsForNodesAsync(
        IReadOnlyCollection<Guid> nodeIds, IReadOnlyCollection<KnowledgeRelationshipType>? typeFilter, CancellationToken cancellationToken);
    Task<List<KnowledgeRelationship>> GetRelationshipsByTypesAsync(
        IReadOnlyCollection<KnowledgeRelationshipType> types, int maxCount, CancellationToken cancellationToken);
    Task AddRelationshipAsync(KnowledgeRelationship relationship, CancellationToken cancellationToken);
    void RemoveRelationship(KnowledgeRelationship relationship);
    void RemoveRelationships(IEnumerable<KnowledgeRelationship> relationships);

    // Existing-entity resolution (reused modules)
    Task<string?> ResolveExternalLabelAsync(KnowledgeNodeType type, Guid externalEntityId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
