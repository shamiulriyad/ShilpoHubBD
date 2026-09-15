using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/unesco-records")]
public class UnescoRecordsController : ControllerBase
{
    private readonly IUnescoRecordService _unescoRecordService;

    public UnescoRecordsController(IUnescoRecordService unescoRecordService)
    {
        _unescoRecordService = unescoRecordService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UnescoRecordDto>>> GetAll(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _unescoRecordService.GetAllAsync(includeInactive, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UnescoRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _unescoRecordService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<UnescoRecordDto>> Create(
        CreateUnescoRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _unescoRecordService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UnescoRecordDto>> Update(
        Guid id, UpdateUnescoRecordRequest request, CancellationToken cancellationToken)
        => Ok(await _unescoRecordService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _unescoRecordService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
