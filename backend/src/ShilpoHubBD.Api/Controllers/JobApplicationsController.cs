using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/job-applications")]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobApplicationsController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<JobApplicationDto>> Apply(CreateJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.ApplyAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobApplicationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<JobApplicationListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.GetMyApplicationsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("job-listings/{jobListingId:guid}")]
    public async Task<ActionResult<List<JobApplicationListItemDto>>> GetByJobListing(Guid jobListingId, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.GetByJobListingAsync(CurrentUserId, jobListingId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/candidate-portfolio")]
    public async Task<ActionResult<PortfolioDto>> GetCandidatePortfolio(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.GetCandidatePortfolioAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/shortlist")]
    public async Task<ActionResult<JobApplicationDto>> Shortlist(
        Guid id, RespondJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.ShortlistAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<JobApplicationDto>> Reject(
        Guid id, RespondJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.RejectAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/hire")]
    public async Task<ActionResult<JobApplicationDto>> Hire(
        Guid id, RespondJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.HireAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<JobApplicationDto>> Withdraw(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobApplicationService.WithdrawAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}
