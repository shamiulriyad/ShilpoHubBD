using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Repositories;

public class SurveyRepository : ISurveyRepository
{
    private readonly ShilpoHubDbContext _context;

    public SurveyRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Surveys ---------------------------------------------------------

    public Task<Survey?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Surveys
            .Include(s => s.Owner)
            .Include(s => s.FieldAssignments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Survey?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        => _context.Surveys
            .Include(s => s.Owner)
            .Include(s => s.Questions)
            .Include(s => s.FieldAssignments).ThenInclude(a => a.FieldResearcher)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
        => _context.Surveys.AnyAsync(s => s.Slug == slug, cancellationToken);

    public async Task<(List<Survey> Items, int TotalCount)> GetPagedForUserAsync(
        Guid userId, SurveyQueryParameters query, CancellationToken cancellationToken)
    {
        var surveys = _context.Surveys
            .Include(s => s.Owner)
            .Include(s => s.Questions)
            .Include(s => s.FieldAssignments)
            .AsSplitQuery()
            .AsQueryable();

        var scope = query.Scope?.Trim().ToLowerInvariant();
        surveys = scope switch
        {
            "owned" => surveys.Where(s => s.OwnerUserId == userId),
            "assigned" => surveys.Where(s => s.FieldAssignments.Any(a => a.FieldResearcherUserId == userId && a.IsActive)),
            _ => surveys.Where(s => s.OwnerUserId == userId
                || s.FieldAssignments.Any(a => a.FieldResearcherUserId == userId && a.IsActive)),
        };

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<SurveyStatus>(query.Status, true, out var status))
        {
            surveys = surveys.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            surveys = surveys.Where(s => s.Title.ToLower().Contains(term) || s.Description.ToLower().Contains(term));
        }

        surveys = surveys.OrderByDescending(s => s.UpdatedAt);

        var totalCount = await surveys.CountAsync(cancellationToken);
        var items = await surveys
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Survey survey, CancellationToken cancellationToken)
        => await _context.Surveys.AddAsync(survey, cancellationToken);

    public void Remove(Survey survey)
        => _context.Surveys.Remove(survey);

    // ---- Questions -----------------------------------------------------

    public Task<SurveyQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken)
        => _context.SurveyQuestions.FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

    public Task<List<SurveyQuestion>> GetQuestionsAsync(Guid surveyId, CancellationToken cancellationToken)
        => _context.SurveyQuestions
            .Where(q => q.SurveyId == surveyId)
            .OrderBy(q => q.OrderIndex)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> QuestionHasAnswersAsync(Guid questionId, CancellationToken cancellationToken)
        => _context.SurveyResponseAnswers.AnyAsync(a => a.SurveyQuestionId == questionId, cancellationToken);

    public async Task AddQuestionAsync(SurveyQuestion question, CancellationToken cancellationToken)
        => await _context.SurveyQuestions.AddAsync(question, cancellationToken);

    public void RemoveQuestion(SurveyQuestion question)
        => _context.SurveyQuestions.Remove(question);

    // ---- Assignments -------------------------------------------------

    public Task<SurveyFieldAssignment?> GetAssignmentAsync(Guid surveyId, Guid userId, CancellationToken cancellationToken)
        => _context.SurveyFieldAssignments
            .Include(a => a.FieldResearcher)
            .FirstOrDefaultAsync(a => a.SurveyId == surveyId && a.FieldResearcherUserId == userId, cancellationToken);

    public Task<SurveyFieldAssignment?> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken)
        => _context.SurveyFieldAssignments
            .Include(a => a.FieldResearcher)
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

    public Task<List<SurveyFieldAssignment>> GetAssignmentsAsync(Guid surveyId, CancellationToken cancellationToken)
        => _context.SurveyFieldAssignments
            .Include(a => a.FieldResearcher)
            .Where(a => a.SurveyId == surveyId)
            .OrderBy(a => a.AssignedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAssignmentAsync(SurveyFieldAssignment assignment, CancellationToken cancellationToken)
        => await _context.SurveyFieldAssignments.AddAsync(assignment, cancellationToken);

    public void RemoveAssignment(SurveyFieldAssignment assignment)
        => _context.SurveyFieldAssignments.Remove(assignment);

    // ---- Responses --------------------------------------------------

    public Task<SurveyResponse?> GetResponseByIdAsync(Guid responseId, CancellationToken cancellationToken)
        => _context.SurveyResponses
            .Include(r => r.SubmittedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.Answers).ThenInclude(a => a.Question)
            .Include(r => r.Evidence).ThenInclude(e => e.CapturedBy)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == responseId, cancellationToken);

    public async Task<(List<SurveyResponse> Items, int TotalCount)> GetResponsesAsync(
        Guid surveyId, SurveyResponseQueryParameters query, CancellationToken cancellationToken)
    {
        var responses = _context.SurveyResponses
            .Include(r => r.SubmittedBy)
            .Include(r => r.Answers)
            .Include(r => r.Evidence)
            .AsSplitQuery()
            .Where(r => r.SurveyId == surveyId);

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<SurveyResponseStatus>(query.Status, true, out var status))
        {
            responses = responses.Where(r => r.Status == status);
        }

        if (query.SubmittedByUserId.HasValue)
        {
            responses = responses.Where(r => r.SubmittedByUserId == query.SubmittedByUserId.Value);
        }

        responses = responses.OrderByDescending(r => r.CollectedAt).ThenByDescending(r => r.CreatedAt);

        var totalCount = await responses.CountAsync(cancellationToken);
        var items = await responses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddResponseAsync(SurveyResponse response, CancellationToken cancellationToken)
        => await _context.SurveyResponses.AddAsync(response, cancellationToken);

    public void RemoveResponse(SurveyResponse response)
        => _context.SurveyResponses.Remove(response);

    public void RemoveAnswers(IEnumerable<SurveyResponseAnswer> answers)
        => _context.SurveyResponseAnswers.RemoveRange(answers);

    // ---- Evidence --------------------------------------------------

    public Task<FieldEvidence?> GetEvidenceByIdAsync(Guid evidenceId, CancellationToken cancellationToken)
        => _context.FieldEvidence
            .Include(e => e.CapturedBy)
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

    public async Task<(List<FieldEvidence> Items, int TotalCount)> GetEvidenceAsync(
        Guid surveyId, FieldEvidenceQueryParameters query, CancellationToken cancellationToken)
    {
        var evidence = _context.FieldEvidence
            .Include(e => e.CapturedBy)
            .Where(e => e.SurveyId == surveyId);

        if (!string.IsNullOrWhiteSpace(query.EvidenceType)
            && Enum.TryParse<FieldEvidenceType>(query.EvidenceType, true, out var type))
        {
            evidence = evidence.Where(e => e.EvidenceType == type);
        }

        if (query.SurveyResponseId.HasValue)
        {
            evidence = evidence.Where(e => e.SurveyResponseId == query.SurveyResponseId.Value);
        }

        evidence = evidence.OrderByDescending(e => e.CapturedAt);

        var totalCount = await evidence.CountAsync(cancellationToken);
        var items = await evidence
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddEvidenceAsync(FieldEvidence evidence, CancellationToken cancellationToken)
        => await _context.FieldEvidence.AddAsync(evidence, cancellationToken);

    public void RemoveEvidence(FieldEvidence evidence)
        => _context.FieldEvidence.Remove(evidence);

    // ---- History --------------------------------------------------

    public async Task AddEventAsync(DataCollectionEvent collectionEvent, CancellationToken cancellationToken)
        => await _context.DataCollectionEvents.AddAsync(collectionEvent, cancellationToken);

    public Task<List<DataCollectionEvent>> GetEventsAsync(Guid surveyId, int take, CancellationToken cancellationToken)
        => _context.DataCollectionEvents
            .Include(e => e.Actor)
            .Where(e => e.SurveyId == surveyId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
