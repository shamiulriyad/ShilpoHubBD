using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/exams")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("courses/{courseId:guid}")]
    public async Task<ActionResult<ExamDto>> Create(Guid courseId, CreateExamRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.CreateAsync(CurrentUserId, courseId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<List<ExamListItemDto>>> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _examService.GetByCourseAsync(CurrentUserId, courseId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _examService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExamDto>> Update(Guid id, UpdateExamRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _examService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult<ExamQuestionDto>> AddQuestion(Guid id, CreateExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.AddQuestionAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult<ExamQuestionDto>> UpdateQuestion(
        Guid id, Guid questionId, UpdateExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.UpdateQuestionAsync(CurrentUserId, id, questionId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid id, Guid questionId, CancellationToken cancellationToken)
    {
        await _examService.DeleteQuestionAsync(CurrentUserId, id, questionId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/attempts/start")]
    public async Task<ActionResult<ExamAttemptStartDto>> StartAttempt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _examService.StartAttemptAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    public async Task<ActionResult<ExamAttemptResultDto>> SubmitAttempt(
        Guid attemptId, SubmitExamAttemptRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.SubmitAttemptAsync(CurrentUserId, attemptId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("attempts/{attemptId:guid}")]
    public async Task<ActionResult<ExamAttemptResultDto>> GetAttemptResult(Guid attemptId, CancellationToken cancellationToken)
    {
        var result = await _examService.GetAttemptResultAsync(CurrentUserId, attemptId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/attempts/mine")]
    public async Task<ActionResult<List<ExamAttemptListItemDto>>> GetMyAttempts(Guid id, CancellationToken cancellationToken)
    {
        var result = await _examService.GetMyAttemptsAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/attempts")]
    public async Task<ActionResult<List<ExamAttemptListItemDto>>> GetAttempts(Guid id, CancellationToken cancellationToken)
    {
        var result = await _examService.GetAttemptsForTrainerAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("attempts/{attemptId:guid}/questions/{questionId:guid}/evaluate")]
    public async Task<ActionResult<ExamAttemptResultDto>> EvaluateAnswer(
        Guid attemptId, Guid questionId, EvaluateExamAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.EvaluateAnswerAsync(CurrentUserId, attemptId, questionId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("attempts/{attemptId:guid}/finalize")]
    public async Task<ActionResult<ExamAttemptResultDto>> FinalizeEvaluation(Guid attemptId, CancellationToken cancellationToken)
    {
        var result = await _examService.FinalizeEvaluationAsync(CurrentUserId, attemptId, cancellationToken);
        return Ok(result);
    }
}
