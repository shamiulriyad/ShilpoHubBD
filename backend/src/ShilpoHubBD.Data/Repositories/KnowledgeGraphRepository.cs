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
            KnowledgeNodeType.Heritage or KnowledgeNodeType.CulturalSite => await _context.HeritagePlaces
                .Where(h => h.Id == externalEntityId).Select(h => h.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Artisan => await _context.Users
                .Where(u => u.Id == externalEntityId && u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Craft => await _context.CraftHeritageEntries
                .Where(c => c.Id == externalEntityId).Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Material => await _context.Materials
                .Where(m => m.Id == externalEntityId).Select(m => m.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.District => await _context.Districts
                .Where(d => d.Id == externalEntityId).Select(d => d.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.TouristPlace => await _context.TourismLocations
                .Where(t => t.Id == externalEntityId).Select(t => t.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Food => await _context.LocalCuisines
                .Where(f => f.Id == externalEntityId).Select(f => f.Name)
                .FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Festival => await _context.HeritageFestivals
                .Where(f => f.Id == externalEntityId).Select(f => f.Name)
                .FirstOrDefaultAsync(cancellationToken),
            _ => null,
        };

    public async Task<List<KnowledgeEntityCandidateDto>> SearchExternalEntitiesAsync(
        KnowledgeNodeType type, string? search, int take, CancellationToken cancellationToken)
    {
        var term = search?.Trim().ToLower() ?? string.Empty;
        take = Math.Clamp(take, 1, 50);
        IQueryable<KnowledgeEntityCandidateDto>? query = type switch
        {
            KnowledgeNodeType.Producer or KnowledgeNodeType.Artisan => _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer) &&
                    (term == "" || u.FullName.ToLower().Contains(term)))
                .Select(u => new KnowledgeEntityCandidateDto { Id = u.Id, Name = u.FullName, EntityType = type.ToString() }),
            KnowledgeNodeType.Product => _context.Products
                .Where(p => term == "" || p.Name.ToLower().Contains(term))
                .Select(p => new KnowledgeEntityCandidateDto { Id = p.Id, Name = p.Name, EntityType = "Product", Description = p.Description }),
            KnowledgeNodeType.Village => _context.Villages
                .Where(v => term == "" || v.Name.ToLower().Contains(term))
                .Select(v => new KnowledgeEntityCandidateDto { Id = v.Id, Name = v.Name, EntityType = "Village", Description = v.Description }),
            KnowledgeNodeType.District => _context.Districts
                .Where(d => term == "" || d.Name.ToLower().Contains(term))
                .Select(d => new KnowledgeEntityCandidateDto { Id = d.Id, Name = d.Name, EntityType = "District" }),
            KnowledgeNodeType.Craft => _context.CraftHeritageEntries
                .Where(c => term == "" || c.Name.ToLower().Contains(term))
                .Select(c => new KnowledgeEntityCandidateDto { Id = c.Id, Name = c.Name, EntityType = "Craft", Description = c.Summary }),
            KnowledgeNodeType.Material => _context.Materials
                .Where(m => term == "" || m.Name.ToLower().Contains(term))
                .Select(m => new KnowledgeEntityCandidateDto { Id = m.Id, Name = m.Name, EntityType = "Material" }),
            KnowledgeNodeType.Heritage or KnowledgeNodeType.CulturalSite => _context.HeritagePlaces
                .Where(h => term == "" || h.Name.ToLower().Contains(term))
                .Select(h => new KnowledgeEntityCandidateDto { Id = h.Id, Name = h.Name, EntityType = type.ToString(), Description = h.Description }),
            KnowledgeNodeType.TouristPlace => _context.TourismLocations
                .Where(t => term == "" || t.Name.ToLower().Contains(term))
                .Select(t => new KnowledgeEntityCandidateDto { Id = t.Id, Name = t.Name, EntityType = "TouristPlace", Description = t.Description }),
            KnowledgeNodeType.Food => _context.LocalCuisines
                .Where(f => term == "" || f.Name.ToLower().Contains(term))
                .Select(f => new KnowledgeEntityCandidateDto { Id = f.Id, Name = f.Name, EntityType = "Food", Description = f.Description }),
            KnowledgeNodeType.Festival => _context.HeritageFestivals
                .Where(f => term == "" || f.Name.ToLower().Contains(term))
                .Select(f => new KnowledgeEntityCandidateDto { Id = f.Id, Name = f.Name, EntityType = "Festival", Description = f.Description }),
            _ => null,
        };

        if (query is null)
        {
            return new List<KnowledgeEntityCandidateDto>();
        }

        var existingEntityIds = _context.KnowledgeNodes
            .Where(n => n.NodeType == type && n.ExternalEntityId.HasValue)
            .Select(n => n.ExternalEntityId!.Value);

        return await query
            .Where(option => !existingEntityIds.Contains(option.Id))
            .OrderBy(option => option.Name)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<KnowledgeEntityCandidateDto?> ResolveExternalEntityAsync(
        KnowledgeNodeType type, Guid externalEntityId, CancellationToken cancellationToken)
    {
        var name = await ResolveExternalLabelAsync(type, externalEntityId, cancellationToken);
        if (name is null)
        {
            return null;
        }

        string? description = type switch
        {
            KnowledgeNodeType.Product => await _context.Products.Where(x => x.Id == externalEntityId)
                .Select(x => x.Description).FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Village => await _context.Villages.Where(x => x.Id == externalEntityId)
                .Select(x => x.Description).FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Craft => await _context.CraftHeritageEntries.Where(x => x.Id == externalEntityId)
                .Select(x => x.Summary).FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Heritage or KnowledgeNodeType.CulturalSite => await _context.HeritagePlaces
                .Where(x => x.Id == externalEntityId).Select(x => x.Description).FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.TouristPlace => await _context.TourismLocations.Where(x => x.Id == externalEntityId)
                .Select(x => x.Description).FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Food => await _context.LocalCuisines.Where(x => x.Id == externalEntityId)
                .Select(x => x.Description).FirstOrDefaultAsync(cancellationToken),
            KnowledgeNodeType.Festival => await _context.HeritageFestivals.Where(x => x.Id == externalEntityId)
                .Select(x => x.Description).FirstOrDefaultAsync(cancellationToken),
            _ => null,
        };

        return new KnowledgeEntityCandidateDto
        {
            Id = externalEntityId,
            Name = name,
            EntityType = type.ToString(),
            Description = description,
        };
    }

    public async Task<KnowledgeGraphStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        var totalNodes = await _context.KnowledgeNodes.CountAsync(cancellationToken);
        var totalRelationships = await _context.KnowledgeRelationships.CountAsync(cancellationToken);
        var connected = _context.KnowledgeRelationships.Select(r => r.SourceNodeId)
            .Union(_context.KnowledgeRelationships.Select(r => r.TargetNodeId));
        var isolatedNodes = await _context.KnowledgeNodes.CountAsync(n => !connected.Contains(n.Id), cancellationToken);
        return new KnowledgeGraphStatsDto
        {
            TotalNodes = totalNodes,
            TotalRelationships = totalRelationships,
            IsolatedNodes = isolatedNodes,
        };
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
