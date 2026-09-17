using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Heritage Knowledge Graph: browse/traverse nodes and relationships (Cross-Platform AI: public
/// read, a discovery tool for everyone); curating the graph (create/import/update/delete) stays
/// restricted to <see cref="StewardRoles"/>.</summary>
[ApiController]
[Route("api/knowledge-graph")]
public class KnowledgeGraphController : ControllerBase
{
    private const string StewardRoles = $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}";

    private readonly IKnowledgeGraphService _graphService;

    public KnowledgeGraphController(IKnowledgeGraphService graphService)
    {
        _graphService = graphService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsSuperAdmin => User.IsInRole(RoleNames.SuperAdmin);

    // ---- Nodes ----

    [HttpGet("nodes")]
    public async Task<ActionResult<PagedResult<KnowledgeNodeDto>>> GetNodes(
        [FromQuery] KnowledgeNodeQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _graphService.GetNodesAsync(query, cancellationToken));

    [HttpGet("nodes/{id:guid}")]
    public async Task<ActionResult<KnowledgeNodeDto>> GetNode(Guid id, CancellationToken cancellationToken)
        => Ok(await _graphService.GetNodeByIdAsync(id, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpPost("nodes")]
    public async Task<ActionResult<KnowledgeNodeDto>> CreateNode(
        CreateKnowledgeNodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _graphService.CreateNodeAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetNode), new { id = result.Id }, result);
    }

    [Authorize(Roles = StewardRoles)]
    [HttpPost("nodes/import")]
    public async Task<ActionResult<KnowledgeNodeDto>> ImportNode(
        ImportKnowledgeNodeRequest request, CancellationToken cancellationToken)
        => Ok(await _graphService.ImportNodeAsync(CurrentUserId, request, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpPut("nodes/{id:guid}")]
    public async Task<ActionResult<KnowledgeNodeDto>> UpdateNode(
        Guid id, UpdateKnowledgeNodeRequest request, CancellationToken cancellationToken)
        => Ok(await _graphService.UpdateNodeAsync(CurrentUserId, id, request, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpDelete("nodes/{id:guid}")]
    public async Task<IActionResult> DeleteNode(Guid id, CancellationToken cancellationToken)
    {
        await _graphService.DeleteNodeAsync(CurrentUserId, IsSuperAdmin, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("nodes/{id:guid}/neighbors")]
    public async Task<ActionResult<KnowledgeGraphDto>> GetNeighbors(Guid id, CancellationToken cancellationToken)
        => Ok(await _graphService.GetNeighborsAsync(id, cancellationToken));

    [HttpGet("nodes/{id:guid}/traverse")]
    public async Task<ActionResult<KnowledgeGraphDto>> Traverse(
        Guid id, [FromQuery] GraphTraversalQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _graphService.TraverseAsync(id, query, cancellationToken));

    // ---- Relationships ----

    [HttpGet("relationships")]
    public async Task<ActionResult<PagedResult<KnowledgeRelationshipDto>>> GetRelationships(
        [FromQuery] KnowledgeRelationshipQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _graphService.GetRelationshipsAsync(query, cancellationToken));

    [HttpGet("relationships/{id:guid}")]
    public async Task<ActionResult<KnowledgeRelationshipDto>> GetRelationship(Guid id, CancellationToken cancellationToken)
        => Ok(await _graphService.GetRelationshipByIdAsync(id, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpPost("relationships")]
    public async Task<ActionResult<KnowledgeRelationshipDto>> CreateRelationship(
        CreateKnowledgeRelationshipRequest request, CancellationToken cancellationToken)
    {
        var result = await _graphService.CreateRelationshipAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetRelationship), new { id = result.Id }, result);
    }

    [Authorize(Roles = StewardRoles)]
    [HttpPut("relationships/{id:guid}")]
    public async Task<ActionResult<KnowledgeRelationshipDto>> UpdateRelationship(
        Guid id, UpdateKnowledgeRelationshipRequest request, CancellationToken cancellationToken)
        => Ok(await _graphService.UpdateRelationshipAsync(CurrentUserId, id, request, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpDelete("relationships/{id:guid}")]
    public async Task<IActionResult> DeleteRelationship(Guid id, CancellationToken cancellationToken)
    {
        await _graphService.DeleteRelationshipAsync(CurrentUserId, IsSuperAdmin, id, cancellationToken);
        return NoContent();
    }

    // ---- Traversal / networks ----

    [HttpGet("paths")]
    public async Task<ActionResult<KnowledgePathDto>> FindPath(
        [FromQuery] GraphPathQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _graphService.FindPathAsync(query, cancellationToken));

    [HttpGet("networks/{network}")]
    public async Task<ActionResult<KnowledgeGraphDto>> GetNetwork(
        string network, [FromQuery] KnowledgeNetworkQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _graphService.GetNetworkAsync(network, query, cancellationToken));
}
