using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("courses/{courseId:guid}")]
    public async Task<ActionResult<QuizDto>> Create(Guid courseId, CreateQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.CreateAsync(CurrentUserId, courseId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<List<QuizListItemDto>>> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetByCourseAsync(CurrentUserId, courseId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuizDto>> Update(Guid id, UpdateQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _quizService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult<QuizQuestionDto>> AddQuestion(Guid id, CreateQuizQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.AddQuestionAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult<QuizQuestionDto>> UpdateQuestion(
        Guid id, Guid questionId, UpdateQuizQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.UpdateQuestionAsync(CurrentUserId, id, questionId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid id, Guid questionId, CancellationToken cancellationToken)
    {
        await _quizService.DeleteQuestionAsync(CurrentUserId, id, questionId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/attempts/start")]
    public async Task<ActionResult<QuizAttemptStartDto>> StartAttempt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quizService.StartAttemptAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    public async Task<ActionResult<QuizAttemptResultDto>> SubmitAttempt(
        Guid attemptId, SubmitQuizAttemptRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.SubmitAttemptAsync(CurrentUserId, attemptId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("attempts/{attemptId:guid}")]
    public async Task<ActionResult<QuizAttemptResultDto>> GetAttemptResult(Guid attemptId, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetAttemptResultAsync(CurrentUserId, attemptId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/attempts/mine")]
    public async Task<ActionResult<List<QuizAttemptListItemDto>>> GetMyAttempts(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetMyAttemptsAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/attempts")]
    public async Task<ActionResult<List<QuizAttemptListItemDto>>> GetAttempts(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetAttemptsForTrainerAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}
