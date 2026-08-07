using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<PagedResult<QuestionDto>>> GetByProduct(
        Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _questionService.GetByProductAsync(productId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("product/{productId:guid}")]
    public async Task<ActionResult<QuestionDto>> Ask(Guid productId, CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.AskAsync(productId, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/answers")]
    public async Task<ActionResult<QuestionDto>> Answer(Guid id, CreateAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.AnswerAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid id, CancellationToken cancellationToken)
    {
        await _questionService.DeleteQuestionAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/answers/{answerId:guid}")]
    public async Task<IActionResult> DeleteAnswer(Guid id, Guid answerId, CancellationToken cancellationToken)
    {
        await _questionService.DeleteAnswerAsync(id, answerId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}
