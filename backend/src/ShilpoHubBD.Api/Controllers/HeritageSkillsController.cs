using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/heritage-skills")]
public class HeritageSkillsController : ControllerBase
{
    private readonly IHeritageSkillService _skillService;

    public HeritageSkillsController(IHeritageSkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<ActionResult<List<HeritageSkillDto>>> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _skillService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<HeritageSkillDto>> Create(CreateHeritageSkillRequest request, CancellationToken cancellationToken)
    {
        var result = await _skillService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }
}
