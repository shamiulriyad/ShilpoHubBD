using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Data.Repositories;

public class KnowledgeGraphRepository : IKnowledgeGraphRepository
{
    private readonly ShilpoHubDbContext _context;

    public KnowledgeGraphRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Nodes ----------------------------------------------------------

    public Task<KnowledgeNode?> GetNodeByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.KnowledgeNodes
            .Include(n => n.CreatedBy)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<List<KnowledgeNode>> GetNodesByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new List<KnowledgeNode>();
        }

        return await _context.KnowledgeNodes
            .Include(n => n.CreatedBy)
            .Where(n => ids.Contains(n.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<KnowledgeNode?> GetNodeByExternalAsync(
        KnowledgeNodeType type, Guid externalEntityId, CancellationToken cancellationToken)
        => _context.KnowledgeNodes
            .FirstOrDefaultAsync(n => n.NodeType == type && n.ExternalEntityId == externalEntityId, cancellationToken);

    public Task<KnowledgeNode?> GetNodeByLabelAsync(
        KnowledgeNodeType type, string labelNormalized, CancellationToken cancellationToken)
        => _context.KnowledgeNodes
            .FirstOrDefaultAsync(n => n.NodeType == type && n.LabelNormalized == labelNormalized, cancellationToken);

    public async Task<(List<KnowledgeNode> Items, int TotalCount)> GetNodesPagedAsync(
        KnowledgeNodeQueryParameters query, CancellationToken cancellationToken)
    {
        var nodes = _context.KnowledgeNodes.Include(n => n.CreatedBy).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.NodeType)
            && Enum.TryParse<KnowledgeNodeType>(query.NodeType, true, out var nodeType))
        {
            nodes = nodes.Where(n => n.NodeType == nodeType);
        }

        if (query.HasExternalEntity == true)
        {
            nodes = nodes.Where(n => n.ExternalEntityId != null);
        }
        else if (query.HasExternalEntity == false)
        {
            nodes = nodes.Where(n => n.ExternalEntityId == null);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            nodes = nodes.Where(n => n.LabelNormalized.Contains(term)
                || (n.Description != null && n.Description.ToLower().Contains(term)));
        }

        nodes = nodes.OrderBy(n => n.NodeType).ThenBy(n => n.Label);

        var totalCount = await nodes.CountAsync(cancellationToken);
        var items = await nodes
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<int> CountRelationshipsForNodeAsync(Guid nodeId, CancellationToken cancellationToken)
        => _context.KnowledgeRelationships
            .CountAsync(r => r.SourceNodeId == nodeId || r.TargetNodeId == nodeId, cancellationToken);

    public async Task AddNodeAsync(KnowledgeNode node, CancellationToken cancellationToken)
        => await _context.KnowledgeNodes.AddAsync(node, cancellationToken);

    public void RemoveNode(KnowledgeNode node)
        => _context.KnowledgeNodes.Remove(node);

    // ---- Relationships ---------------------------------------------

    private IQueryable<KnowledgeRelationship> RelationshipsWithNodes()
        => _context.KnowledgeRelationships
            .Include(r => r.CreatedBy)
            .Include(r => r.SourceNode)
            .Include(r => r.TargetNode);

    public Task<KnowledgeRelationship?> GetRelationshipByIdAsync(Guid id, CancellationToken cancellationToken)
        => RelationshipsWithNodes().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<KnowledgeRelationship?> GetRelationshipAsync(
        Guid sourceNodeId, Guid targetNodeId, KnowledgeRelationshipType type, CancellationToken cancellationToken)
        => _context.KnowledgeRelationships.FirstOrDefaultAsync(
            r => r.SourceNodeId == sourceNodeId && r.TargetNodeId == targetNodeId && r.RelationshipType == type,
            cancellationToken);

    public async Task<(List<KnowledgeRelationship> Items, int TotalCount)> GetRelationshipsPagedAsync(
        KnowledgeRelationshipQueryParameters query, CancellationToken cancellationToken)
    {
        var relationships = RelationshipsWithNodes();

        if (!string.IsNullOrWhiteSpace(query.RelationshipType)
            && Enum.TryParse<KnowledgeRelationshipType>(query.RelationshipType, true, out var type))
        {
            relationships = relationships.Where(r => r.RelationshipType == type);
        }

        if (query.NodeId.HasValue)
        {
            relationships = relationships.Where(r =>
                r.SourceNodeId == query.NodeId.Value || r.TargetNodeId == query.NodeId.Value);
        }

        if (query.SourceNodeId.HasValue)
        {
            relationships = relationships.Where(r => r.SourceNodeId == query.SourceNodeId.Value);
        }

        if (query.TargetNodeId.HasValue)
        {
            relationships = relationships.Where(r => r.TargetNodeId == query.TargetNodeId.Value);
        }

        relationships = relationships.OrderByDescending(r => r.CreatedAt);

        var totalCount = await relationships.CountAsync(cancellationToken);
        var items = await relationships
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<KnowledgeRelationship>> GetRelationshipsForNodesAsync(
        IReadOnlyCollection<Guid> nodeIds,
        IReadOnlyCollection<KnowledgeRelationshipType>? typeFilter,
        CancellationToken cancellationToken)
    {
        if (nodeIds.Count == 0)
        {
            return new List<KnowledgeRelationship>();
        }

        var query = _context.KnowledgeRelationships
            .Where(r => nodeIds.Contains(r.SourceNodeId) || nodeIds.Contains(r.TargetNodeId));

        if (typeFilter is { Count: > 0 })
        {
            query = query.Where(r => typeFilter.Contains(r.RelationshipType));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<KnowledgeRelationship>> GetRelationshipsByTypesAsync(
        IReadOnlyCollection<KnowledgeRelationshipType> types, int maxCount, CancellationToken cancellationToken)
    {
        if (types.Count == 0)
        {
            return new List<KnowledgeRelationship>();
        }

        return await _context.KnowledgeRelationships
            .Where(r => types.Contains(r.RelationshipType))
            .OrderByDescending(r => r.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRelationshipAsync(KnowledgeRelationship relationship, CancellationToken cancellationToken)
        => await _context.KnowledgeRelationships.AddAsync(relationship, cancellationToken);

    public void RemoveRelationship(KnowledgeRelationship relationship)
        => _context.KnowledgeRelationships.Remove(relationship);

    public void RemoveRelationships(IEnumerable<KnowledgeRelationship> relationships)
        => _context.KnowledgeRelationships.RemoveRange(relationships);

    // ---- External entity resolution ----------------------------

    public async Task<string?> ResolveExternalLabelAsync(
        KnowledgeNodeType type, Guid externalEntityId, CancellationToken cancellationToken)
        => type switch
        {
            KnowledgeNodeType.Producer => await _context.Users
                .Where(u => u.Id == externalEntityId && u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Village => await _context.Villages
                .Where(v => v.Id == externalEntityId).Select(v => v.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Product => await _context.Products
                .Where(p => p.Id == externalEntityId).Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.HeritagePlace => await _context.HeritagePlaces
                .Where(h => h.Id == externalEntityId).Select(h => h.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Family => await _context.ProducerHeritageIdentities
                .Where(f => f.Id == externalEntityId)
                .Select(f => f.WorkshopName != "" ? f.WorkshopName : f.HeritageIdNumber)
                .FirstOrDefaultAsync(cancellationToken),
            _ => null,
        };

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
