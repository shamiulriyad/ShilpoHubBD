using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/governance/funding/applications")]
public class FundingApplicationsController : ControllerBase
{
    private readonly IFundingService _fundingService;

    public FundingApplicationsController(IFundingService fundingService)
    {
        _fundingService = fundingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<FundingApplicationListItemDto>>> GetPaged(
        [FromQuery] FundingApplicationQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _fundingService.GetApplicationsAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FundingApplicationDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _fundingService.GetApplicationByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<FundingApplicationDto>> Create(
        CreateFundingApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _fundingService.CreateApplicationAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/reviews")]
    public async Task<ActionResult<FundingApplicationDto>> AddReview(
        Guid id, SubmitFundingReviewRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.AddReviewAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/decision")]
    public async Task<ActionResult<FundingApplicationDto>> Decide(
        Guid id, DecideFundingApplicationRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.DecideAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<FundingApplicationDto>> Withdraw(
        Guid id, WithdrawFundingApplicationRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.WithdrawAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<FundingApplicationDto>> AddNote(
        Guid id, AddFundingApplicationNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.AddNoteAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/disbursements")]
    public async Task<ActionResult<FundingApplicationDto>> ScheduleDisbursement(
        Guid id, ScheduleFundingDisbursementRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.ScheduleDisbursementAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/disbursements/{disbursementId:guid}/status")]
    public async Task<ActionResult<FundingApplicationDto>> UpdateDisbursementStatus(
        Guid id, Guid disbursementId, UpdateFundingDisbursementStatusRequest request,
        CancellationToken cancellationToken)
        => Ok(await _fundingService.UpdateDisbursementStatusAsync(
            CurrentUserId, id, disbursementId, request, cancellationToken));

    [HttpPost("{id:guid}/repayments")]
    public async Task<ActionResult<FundingApplicationDto>> RecordRepayment(
        Guid id, RecordLoanRepaymentRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.RecordRepaymentAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _fundingService.DeleteApplicationAsync(id, cancellationToken);
        return NoContent();
    }
}
