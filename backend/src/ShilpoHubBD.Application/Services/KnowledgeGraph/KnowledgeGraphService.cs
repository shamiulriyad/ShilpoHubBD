using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Services.KnowledgeGraph;

public class KnowledgeGraphService : IKnowledgeGraphService
{
    private const int MaxTraversalDepth = 6;
    private const int MaxTraversalNodes = 1000;

    private static readonly IReadOnlyDictionary<KnowledgeNetwork, KnowledgeRelationshipType[]> NetworkTypes =
        new Dictionary<KnowledgeNetwork, KnowledgeRelationshipType[]>
        {
            [KnowledgeNetwork.ProducerRelationships] = new[]
            {
                KnowledgeRelationshipType.MentoredBy, KnowledgeRelationshipType.CollaboratesWith,
                KnowledgeRelationshipType.DescendedFrom, KnowledgeRelationshipType.BelongsToFamily,
            },
            [KnowledgeNetwork.VillageConnections] = new[]
            {
                KnowledgeRelationshipType.LocatedInVillage, KnowledgeRelationshipType.VillageHasCulture,
                KnowledgeRelationshipType.AssociatedWith,
            },
            [KnowledgeNetwork.MaterialNetwork] = new[]
            {
                KnowledgeRelationshipType.CraftUsesMaterial, KnowledgeRelationshipType.SuppliesMaterialTo,
            },
            [KnowledgeNetwork.CulturalNetwork] = new[]
            {
                KnowledgeRelationshipType.VillageHasCulture, KnowledgeRelationshipType.PractisesCraft,
                KnowledgeRelationshipType.AssociatedWith,
            },
            [KnowledgeNetwork.FamilyTree] = new[]
            {
                KnowledgeRelationshipType.BelongsToFamily, KnowledgeRelationshipType.DescendedFrom,
            },
        };

    private static readonly KnowledgeNodeType[] ExternalBackedTypes =
    {
        KnowledgeNodeType.Producer, KnowledgeNodeType.Village, KnowledgeNodeType.Product,
        KnowledgeNodeType.HeritagePlace, KnowledgeNodeType.Family,
    };

    private readonly IKnowledgeGraphRepository _repository;

    public KnowledgeGraphService(IKnowledgeGraphRepository repository)
    {
        _repository = repository;
    }

    // ---- Nodes --------------------------------------------------------

    public async Task<PagedResult<KnowledgeNodeDto>> GetNodesAsync(
        KnowledgeNodeQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 25 : query.PageSize;

        var (items, totalCount) = await _repository.GetNodesPagedAsync(query, cancellationToken);
        var rels = await _repository.GetRelationshipsForNodesAsync(
            items.Select(n => n.Id).ToList(), null, cancellationToken);

        var dtos = items.Select(n => n.ToDto(
            rels.Count(r => r.SourceNodeId == n.Id),
            rels.Count(r => r.TargetNodeId == n.Id))).ToList();

        return new PagedResult<KnowledgeNodeDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<KnowledgeNodeDto> GetNodeByIdAsync(Guid nodeId, CancellationToken cancellationToken)
    {
        var node = await _repository.GetNodeByIdAsync(nodeId, cancellationToken)
            ?? throw new NotFoundException("Knowledge node not found.");
        var rels = await _repository.GetRelationshipsForNodesAsync(new[] { nodeId }, null, cancellationToken);
        return node.ToDto(rels.Count(r => r.SourceNodeId == nodeId), rels.Count(r => r.TargetNodeId == nodeId));
    }

    public async Task<KnowledgeNodeDto> CreateNodeAsync(
        Guid userId, CreateKnowledgeNodeRequest request, CancellationToken cancellationToken)
    {
        var type = ParseNodeType(request.NodeType);
        var label = request.Label.Trim();
        if (label.Length == 0)
        {
            throw new ConflictException("Label is required.");
        }

        if (request.ExternalEntityId.HasValue)
        {
            await EnsureExternalEntityAsync(type, request.ExternalEntityId.Value, cancellationToken);
            var existingExternal = await _repository.GetNodeByExternalAsync(type, request.ExternalEntityId.Value, cancellationToken);
            if (existingExternal is not null)
            {
                throw new ConflictException("A node for this entity already exists.");
            }
        }

        var normalized = Normalize(label);
        if (await _repository.GetNodeByLabelAsync(type, normalized, cancellationToken) is not null)
        {
            throw new ConflictException($"A {type} node with this label already exists.");
        }

        var now = DateTime.UtcNow;
        var node = new KnowledgeNode
        {
            Id = Guid.NewGuid(),
            NodeType = type,
            Label = label,
            LabelNormalized = normalized,
            ExternalEntityId = request.ExternalEntityId,
            Description = request.Description?.Trim(),
            MetadataJson = request.MetadataJson,
            IsCurated = true,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddNodeAsync(node, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetNodeByIdAsync(node.Id, cancellationToken))!.ToDto();
    }

    public async Task<KnowledgeNodeDto> ImportNodeAsync(
        Guid userId, ImportKnowledgeNodeRequest request, CancellationToken cancellationToken)
    {
        var type = ParseNodeType(request.NodeType);
        if (!ExternalBackedTypes.Contains(type))
        {
            throw new ConflictException(
                "Import is only supported for Producer, Village, Product, HeritagePlace and Family nodes.");
        }

        var existing = await _repository.GetNodeByExternalAsync(type, request.ExternalEntityId, cancellationToken);
        if (existing is not null)
        {
            return existing.ToDto();
        }

        var resolvedLabel = await _repository.ResolveExternalLabelAsync(type, request.ExternalEntityId, cancellationToken)
            ?? throw new NotFoundException($"No {type} entity was found for the supplied id.");

        var label = string.IsNullOrWhiteSpace(request.LabelOverride) ? resolvedLabel : request.LabelOverride!.Trim();
        var normalized = Normalize(label);

        // Keep the label unique within the type; disambiguate with a short id suffix if needed.
        if (await _repository.GetNodeByLabelAsync(type, normalized, cancellationToken) is not null)
        {
            label = $"{label} ({request.ExternalEntityId.ToString()[..8]})";
            normalized = Normalize(label);
        }

        var now = DateTime.UtcNow;
        var node = new KnowledgeNode
        {
            Id = Guid.NewGuid(),
            NodeType = type,
            Label = label,
            LabelNormalized = normalized,
            ExternalEntityId = request.ExternalEntityId,
            Description = request.Description?.Trim(),
            IsCurated = true,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddNodeAsync(node, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetNodeByIdAsync(node.Id, cancellationToken))!.ToDto();
    }

    public async Task<KnowledgeNodeDto> UpdateNodeAsync(
        Guid userId, Guid nodeId, UpdateKnowledgeNodeRequest request, CancellationToken cancellationToken)
    {
        var node = await _repository.GetNodeByIdAsync(nodeId, cancellationToken)
            ?? throw new NotFoundException("Knowledge node not found.");

        var label = request.Label.Trim();
        if (label.Length == 0)
        {
            throw new ConflictException("Label is required.");
        }

        var normalized = Normalize(label);
        if (normalized != node.LabelNormalized)
        {
            var clash = await _repository.GetNodeByLabelAsync(node.NodeType, normalized, cancellationToken);
            if (clash is not null && clash.Id != node.Id)
            {
                throw new ConflictException($"A {node.NodeType} node with this label already exists.");
            }
        }

        node.Label = label;
        node.LabelNormalized = normalized;
        node.Description = request.Description?.Trim();
        node.MetadataJson = request.MetadataJson;
        node.IsCurated = request.IsCurated;
        node.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetNodeByIdAsync(node.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteNodeAsync(Guid userId, bool isSuperAdmin, Guid nodeId, CancellationToken cancellationToken)
    {
        var node = await _repository.GetNodeByIdAsync(nodeId, cancellationToken)
            ?? throw new NotFoundException("Knowledge node not found.");

        if (node.CreatedByUserId != userId && !isSuperAdmin)
        {
            throw new UnauthorizedAccessException("Only the node's creator or a SuperAdmin can delete it.");
        }

        // Remove edges on both sides first (TargetNode FK is Restrict).
        var edges = await _repository.GetRelationshipsForNodesAsync(new[] { nodeId }, null, cancellationToken);
        _repository.RemoveRelationships(edges);
        _repository.RemoveNode(node);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- Relationships --------------------------------------------

    public async Task<PagedResult<KnowledgeRelationshipDto>> GetRelationshipsAsync(
        KnowledgeRelationshipQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 200 ? 50 : query.PageSize;

        var (items, totalCount) = await _repository.GetRelationshipsPagedAsync(query, cancellationToken);
        return new PagedResult<KnowledgeRelationshipDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<KnowledgeRelationshipDto> GetRelationshipByIdAsync(Guid relationshipId, CancellationToken cancellationToken)
    {
        var relationship = await _repository.GetRelationshipByIdAsync(relationshipId, cancellationToken)
            ?? throw new NotFoundException("Relationship not found.");
        return relationship.ToDto();
    }

    public async Task<KnowledgeRelationshipDto> CreateRelationshipAsync(
        Guid userId, CreateKnowledgeRelationshipRequest request, CancellationToken cancellationToken)
    {
        if (request.SourceNodeId == request.TargetNodeId)
        {
            throw new ConflictException("A relationship cannot connect a node to itself.");
        }

        var type = ParseRelationshipType(request.RelationshipType);

        var source = await _repository.GetNodeByIdAsync(request.SourceNodeId, cancellationToken)
            ?? throw new NotFoundException("Source node not found.");
        var target = await _repository.GetNodeByIdAsync(request.TargetNodeId, cancellationToken)
            ?? throw new NotFoundException("Target node not found.");

        if (await _repository.GetRelationshipAsync(source.Id, target.Id, type, cancellationToken) is not null)
        {
            throw new ConflictException("A relationship of this type already exists between these nodes.");
        }

        // Reject the mirror of an existing undirected edge as well.
        if (!request.IsDirected
            && await _repository.GetRelationshipAsync(target.Id, source.Id, type, cancellationToken) is not null)
        {
            throw new ConflictException("An undirected relationship of this type already exists between these nodes.");
        }

        var now = DateTime.UtcNow;
        var relationship = new KnowledgeRelationship
        {
            Id = Guid.NewGuid(),
            SourceNodeId = source.Id,
            TargetNodeId = target.Id,
            RelationshipType = type,
            IsDirected = request.IsDirected,
            Weight = request.Weight,
            Label = request.Label?.Trim(),
            Note = request.Note?.Trim(),
            MetadataJson = request.MetadataJson,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddRelationshipAsync(relationship, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetRelationshipByIdAsync(relationship.Id, cancellationToken))!.ToDto();
    }

    public async Task<KnowledgeRelationshipDto> UpdateRelationshipAsync(
        Guid userId, Guid relationshipId, UpdateKnowledgeRelationshipRequest request, CancellationToken cancellationToken)
    {
        var relationship = await _repository.GetRelationshipByIdAsync(relationshipId, cancellationToken)
            ?? throw new NotFoundException("Relationship not found.");

        var type = ParseRelationshipType(request.RelationshipType);
        if (type != relationship.RelationshipType)
        {
            var clash = await _repository.GetRelationshipAsync(
                relationship.SourceNodeId, relationship.TargetNodeId, type, cancellationToken);
            if (clash is not null && clash.Id != relationship.Id)
            {
                throw new ConflictException("A relationship of this type already exists between these nodes.");
            }
        }

        relationship.RelationshipType = type;
        relationship.IsDirected = request.IsDirected;
        relationship.Weight = request.Weight;
        relationship.Label = request.Label?.Trim();
        relationship.Note = request.Note?.Trim();
        relationship.MetadataJson = request.MetadataJson;
        relationship.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetRelationshipByIdAsync(relationship.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteRelationshipAsync(
        Guid userId, bool isSuperAdmin, Guid relationshipId, CancellationToken cancellationToken)
    {
        var relationship = await _repository.GetRelationshipByIdAsync(relationshipId, cancellationToken)
            ?? throw new NotFoundException("Relationship not found.");

        if (relationship.CreatedByUserId != userId && !isSuperAdmin)
        {
            throw new UnauthorizedAccessException("Only the relationship's creator or a SuperAdmin can delete it.");
        }

        _repository.RemoveRelationship(relationship);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- Queries / traversal ------------------------------------

    public async Task<KnowledgeGraphDto> GetNeighborsAsync(Guid nodeId, CancellationToken cancellationToken)
    {
        var node = await _repository.GetNodeByIdAsync(nodeId, cancellationToken)
            ?? throw new NotFoundException("Knowledge node not found.");

        var edges = await _repository.GetRelationshipsForNodesAsync(new[] { nodeId }, null, cancellationToken);
        var neighborIds = edges.Select(e => e.SourceNodeId).Concat(edges.Select(e => e.TargetNodeId)).ToHashSet();
        neighborIds.Add(nodeId);

        var nodes = await _repository.GetNodesByIdsAsync(neighborIds.ToList(), cancellationToken);
        return BuildGraph(nodes, edges, depthReached: 1, truncated: false);
    }

    public async Task<KnowledgeGraphDto> TraverseAsync(
        Guid nodeId, GraphTraversalQueryParameters query, CancellationToken cancellationToken)
    {
        var start = await _repository.GetNodeByIdAsync(nodeId, cancellationToken)
            ?? throw new NotFoundException("Knowledge node not found.");

        var maxDepth = Math.Clamp(query.Depth, 1, MaxTraversalDepth);
        var maxNodes = Math.Clamp(query.MaxNodes, 2, MaxTraversalNodes);
        var typeFilter = ParseRelationshipTypeCsv(query.RelationshipTypes);
        var direction = (query.Direction ?? "both").Trim().ToLowerInvariant();

        var visited = new HashSet<Guid> { start.Id };
        var frontier = new List<Guid> { start.Id };
        var collectedEdges = new Dictionary<Guid, KnowledgeRelationship>();
        var truncated = false;
        var depthReached = 0;

        while (frontier.Count > 0 && depthReached < maxDepth && visited.Count < maxNodes)
        {
            var edges = await _repository.GetRelationshipsForNodesAsync(frontier, typeFilter, cancellationToken);
            var frontierSet = frontier.ToHashSet();
            var next = new HashSet<Guid>();

            foreach (var edge in edges)
            {
                collectedEdges[edge.Id] = edge;

                foreach (var neighbor in NeighborsOf(edge, frontierSet, direction))
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    if (visited.Count >= maxNodes)
                    {
                        truncated = true;
                        continue;
                    }

                    visited.Add(neighbor);
                    next.Add(neighbor);
                }
            }

            frontier = next.ToList();
            depthReached++;
        }

        if (frontier.Count > 0)
        {
            truncated = true;
        }

        var nodes = await _repository.GetNodesByIdsAsync(visited.ToList(), cancellationToken);
        var edgesInSubgraph = collectedEdges.Values
            .Where(e => visited.Contains(e.SourceNodeId) && visited.Contains(e.TargetNodeId))
            .ToList();

        return BuildGraph(nodes, edgesInSubgraph, depthReached, truncated);
    }

    public async Task<KnowledgePathDto> FindPathAsync(GraphPathQueryParameters query, CancellationToken cancellationToken)
    {
        if (query.SourceNodeId == query.TargetNodeId)
        {
            throw new ConflictException("Source and target nodes must be different.");
        }

        _ = await _repository.GetNodeByIdAsync(query.SourceNodeId, cancellationToken)
            ?? throw new NotFoundException("Source node not found.");
        _ = await _repository.GetNodeByIdAsync(query.TargetNodeId, cancellationToken)
            ?? throw new NotFoundException("Target node not found.");

        var maxDepth = Math.Clamp(query.MaxDepth, 1, MaxTraversalDepth);
        var typeFilter = ParseRelationshipTypeCsv(query.RelationshipTypes);

        var visited = new HashSet<Guid> { query.SourceNodeId };
        var prev = new Dictionary<Guid, (Guid FromNode, KnowledgeRelationship Edge)>();
        var frontier = new List<Guid> { query.SourceNodeId };
        var depth = 0;
        var found = false;

        while (frontier.Count > 0 && depth < maxDepth && !found)
        {
            var edges = await _repository.GetRelationshipsForNodesAsync(frontier, typeFilter, cancellationToken);
            var frontierSet = frontier.ToHashSet();
            var next = new List<Guid>();

            foreach (var edge in edges)
            {
                foreach (var neighbor in NeighborsOf(edge, frontierSet, "both"))
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    var from = frontierSet.Contains(edge.SourceNodeId) ? edge.SourceNodeId : edge.TargetNodeId;
                    visited.Add(neighbor);
                    prev[neighbor] = (from, edge);
                    next.Add(neighbor);

                    if (neighbor == query.TargetNodeId)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            frontier = next;
            depth++;
        }

        if (!found)
        {
            return new KnowledgePathDto { Found = false, Length = 0 };
        }

        var pathEdges = new List<KnowledgeRelationship>();
        var pathNodeIds = new List<Guid> { query.TargetNodeId };
        var cursor = query.TargetNodeId;
        while (cursor != query.SourceNodeId)
        {
            var (fromNode, edge) = prev[cursor];
            pathEdges.Add(edge);
            pathNodeIds.Add(fromNode);
            cursor = fromNode;
        }

        pathNodeIds.Reverse();
        pathEdges.Reverse();

        var nodes = await _repository.GetNodesByIdsAsync(pathNodeIds, cancellationToken);
        var byId = nodes.ToDictionary(n => n.Id);

        return new KnowledgePathDto
        {
            Found = true,
            Length = pathEdges.Count,
            Nodes = pathNodeIds.Where(byId.ContainsKey).Select(id => byId[id].ToDto()).ToList(),
            Relationships = pathEdges.Select(e => MapEdge(e, byId)).ToList(),
        };
    }

    public async Task<KnowledgeGraphDto> GetNetworkAsync(
        string network, KnowledgeNetworkQueryParameters query, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<KnowledgeNetwork>(network, true, out var parsed))
        {
            throw new ConflictException(
                "Network must be one of: ProducerRelationships, VillageConnections, MaterialNetwork, CulturalNetwork, FamilyTree.");
        }

        var types = NetworkTypes[parsed];

        if (query.FocusNodeId.HasValue)
        {
            return await TraverseAsync(query.FocusNodeId.Value, new GraphTraversalQueryParameters
            {
                Depth = query.Depth,
                MaxNodes = query.MaxNodes,
                Direction = "both",
                RelationshipTypes = string.Join(",", types),
            }, cancellationToken);
        }

        var maxNodes = Math.Clamp(query.MaxNodes, 2, MaxTraversalNodes);
        var edges = await _repository.GetRelationshipsByTypesAsync(types, maxNodes * 4, cancellationToken);
        var nodeIds = edges.Select(e => e.SourceNodeId).Concat(edges.Select(e => e.TargetNodeId)).Distinct().ToList();

        var truncated = false;
        if (nodeIds.Count > maxNodes)
        {
            var keep = nodeIds.Take(maxNodes).ToHashSet();
            edges = edges.Where(e => keep.Contains(e.SourceNodeId) && keep.Contains(e.TargetNodeId)).ToList();
            nodeIds = keep.ToList();
            truncated = true;
        }

        var nodes = await _repository.GetNodesByIdsAsync(nodeIds, cancellationToken);
        return BuildGraph(nodes, edges, depthReached: 0, truncated);
    }

    // ---- helpers -----------------------------------------------

    private static IEnumerable<Guid> NeighborsOf(
        KnowledgeRelationship edge, HashSet<Guid> frontier, string direction)
    {
        var treatBoth = !edge.IsDirected || direction == "both";

        if ((treatBoth || direction == "out") && frontier.Contains(edge.SourceNodeId))
        {
            yield return edge.TargetNodeId;
        }

        if ((treatBoth || direction == "in") && frontier.Contains(edge.TargetNodeId))
        {
            yield return edge.SourceNodeId;
        }
    }

    private static KnowledgeGraphDto BuildGraph(
        List<KnowledgeNode> nodes, List<KnowledgeRelationship> edges, int depthReached, bool truncated)
    {
        var byId = nodes.ToDictionary(n => n.Id);
        var outDegree = edges.GroupBy(e => e.SourceNodeId).ToDictionary(g => g.Key, g => g.Count());
        var inDegree = edges.GroupBy(e => e.TargetNodeId).ToDictionary(g => g.Key, g => g.Count());

        return new KnowledgeGraphDto
        {
            Nodes = nodes
                .Select(n => n.ToDto(
                    outDegree.GetValueOrDefault(n.Id),
                    inDegree.GetValueOrDefault(n.Id)))
                .ToList(),
            Relationships = edges.Select(e => MapEdge(e, byId)).ToList(),
            DepthReached = depthReached,
            Truncated = truncated,
        };
    }

    private static KnowledgeRelationshipDto MapEdge(KnowledgeRelationship e, IReadOnlyDictionary<Guid, KnowledgeNode> byId)
    {
        var dto = e.ToDto();
        if (byId.TryGetValue(e.SourceNodeId, out var source))
        {
            dto.SourceLabel = source.Label;
            dto.SourceNodeType = source.NodeType.ToString();
        }

        if (byId.TryGetValue(e.TargetNodeId, out var target))
        {
            dto.TargetLabel = target.Label;
            dto.TargetNodeType = target.NodeType.ToString();
        }

        return dto;
    }

    private async Task EnsureExternalEntityAsync(
        KnowledgeNodeType type, Guid externalEntityId, CancellationToken cancellationToken)
    {
        if (!ExternalBackedTypes.Contains(type))
        {
            throw new ConflictException(
                $"{type} nodes are concept nodes and cannot reference an external entity.");
        }

        var label = await _repository.ResolveExternalLabelAsync(type, externalEntityId, cancellationToken);
        if (label is null)
        {
            throw new NotFoundException($"No {type} entity was found for the supplied id.");
        }
    }

    private static KnowledgeNodeType ParseNodeType(string value)
        => Enum.TryParse<KnowledgeNodeType>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("NodeType is not a valid knowledge node type.");

    private static KnowledgeRelationshipType ParseRelationshipType(string value)
        => Enum.TryParse<KnowledgeRelationshipType>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("RelationshipType is not a valid knowledge relationship type.");

    private static KnowledgeRelationshipType[]? ParseRelationshipTypeCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return null;
        }

        var types = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => Enum.TryParse<KnowledgeRelationshipType>(t, true, out var parsed) ? parsed : (KnowledgeRelationshipType?)null)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .Distinct()
            .ToArray();

        return types.Length == 0 ? null : types;
    }

    private static string Normalize(string label) => SlugGenerator.Generate(label);
}
