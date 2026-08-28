using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IKnowledgeGraphService
{
    // Nodes
    Task<PagedResult<KnowledgeNodeDto>> GetNodesAsync(KnowledgeNodeQueryParameters query, CancellationToken cancellationToken);
    Task<KnowledgeNodeDto> GetNodeByIdAsync(Guid nodeId, CancellationToken cancellationToken);
    Task<KnowledgeNodeDto> CreateNodeAsync(Guid userId, CreateKnowledgeNodeRequest request, CancellationToken cancellationToken);
    Task<KnowledgeNodeDto> ImportNodeAsync(Guid userId, ImportKnowledgeNodeRequest request, CancellationToken cancellationToken);
    Task<KnowledgeNodeDto> UpdateNodeAsync(Guid userId, Guid nodeId, UpdateKnowledgeNodeRequest request, CancellationToken cancellationToken);
    Task DeleteNodeAsync(Guid userId, bool isSuperAdmin, Guid nodeId, CancellationToken cancellationToken);

    // Relationships
    Task<PagedResult<KnowledgeRelationshipDto>> GetRelationshipsAsync(
        KnowledgeRelationshipQueryParameters query, CancellationToken cancellationToken);
    Task<KnowledgeRelationshipDto> GetRelationshipByIdAsync(Guid relationshipId, CancellationToken cancellationToken);
    Task<KnowledgeRelationshipDto> CreateRelationshipAsync(
        Guid userId, CreateKnowledgeRelationshipRequest request, CancellationToken cancellationToken);
    Task<KnowledgeRelationshipDto> UpdateRelationshipAsync(
        Guid userId, Guid relationshipId, UpdateKnowledgeRelationshipRequest request, CancellationToken cancellationToken);
    Task DeleteRelationshipAsync(Guid userId, bool isSuperAdmin, Guid relationshipId, CancellationToken cancellationToken);

    // Queries / traversal
    Task<KnowledgeGraphDto> GetNeighborsAsync(Guid nodeId, CancellationToken cancellationToken);
    Task<KnowledgeGraphDto> TraverseAsync(Guid nodeId, GraphTraversalQueryParameters query, CancellationToken cancellationToken);
    Task<KnowledgePathDto> FindPathAsync(GraphPathQueryParameters query, CancellationToken cancellationToken);
    Task<KnowledgeGraphDto> GetNetworkAsync(string network, KnowledgeNetworkQueryParameters query, CancellationToken cancellationToken);
}
