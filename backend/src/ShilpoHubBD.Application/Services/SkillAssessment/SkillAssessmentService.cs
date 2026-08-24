using ShilpoHubBD.Application.DTOs.SkillAssessment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Assessment;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.SkillAssessment;
using SkillAssessmentEntity = ShilpoHubBD.Domain.Entities.SkillAssessment.SkillAssessment;

namespace ShilpoHubBD.Application.Services.SkillAssessment;

public class SkillAssessmentService : ISkillAssessmentService
{
    private readonly ISkillAssessmentRepository _skillAssessmentRepository;
    private readonly IAcademyMemberProfileRepository _profileRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IExamRepository _examRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAISkillAssessmentProvider _provider;

    public SkillAssessmentService(
        ISkillAssessmentRepository skillAssessmentRepository,
        IAcademyMemberProfileRepository profileRepository,
        IHeritageSkillRepository heritageSkillRepository,
        IEnrollmentRepository enrollmentRepository,
        IQuizRepository quizRepository,
        IExamRepository examRepository,
        IAssignmentRepository assignmentRepository,
        IAISkillAssessmentProvider provider)
    {
        _skillAssessmentRepository = skillAssessmentRepository;
        _profileRepository = profileRepository;
        _heritageSkillRepository = heritageSkillRepository;
        _enrollmentRepository = enrollmentRepository;
        _quizRepository = quizRepository;
        _examRepository = examRepository;
        _assignmentRepository = assignmentRepository;
        _provider = provider;
    }

    public async Task<SkillAssessmentResultDto> RunAssessmentAsync(Guid userId, Guid heritageSkillId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ConflictException("You must have an academy member profile before running a skill assessment.");

        var heritageSkill = await _heritageSkillRepository.GetByIdAsync(heritageSkillId, cancellationToken)
            ?? throw new NotFoundException("Heritage skill not found.");

        var input = await BuildInputAsync(userId, profile, heritageSkill, cancellationToken);
        var result = await _provider.AssessAsync(input, cancellationToken);

        var assessment = new SkillAssessmentEntity
        {
            Id = Guid.NewGuid(),
            AcademyMemberProfileId = profile.Id,
            HeritageSkillId = heritageSkillId,
            Level = result.Level,
            Score = result.Score,
            Summary = result.Summary,
            AssessedAt = DateTime.UtcNow,
        };

        var order = 0;
        foreach (var strength in result.Strengths)
        {
            assessment.Insights.Add(new SkillAssessmentInsight
            {
                Id = Guid.NewGuid(),
                Type = InsightType.Strength,
                Text = strength,
                DisplayOrder = order++,
            });
        }

        order = 0;
        foreach (var weakness in result.Weaknesses)
        {
            assessment.Insights.Add(new SkillAssessmentInsight
            {
                Id = Guid.NewGuid(),
                Type = InsightType.Weakness,
                Text = weakness,
                DisplayOrder = order++,
            });
        }

        order = 0;
        foreach (var recommendation in result.RecommendedSkills)
        {
            assessment.RecommendedSkills.Add(new SkillAssessmentRecommendedSkill
            {
                Id = Guid.NewGuid(),
                HeritageSkillId = recommendation.HeritageSkillId,
                Reason = recommendation.Reason,
                DisplayOrder = order++,
            });
        }

        await _skillAssessmentRepository.AddAsync(assessment, cancellationToken);
        await _skillAssessmentRepository.SaveChangesAsync(cancellationToken);

        var created = await _skillAssessmentRepository.GetByIdAsync(assessment.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<SkillAssessmentResultDto> GetByIdAsync(Guid userId, Guid assessmentId, CancellationToken cancellationToken)
    {
        var assessment = await _skillAssessmentRepository.GetByIdAsync(assessmentId, cancellationToken)
            ?? throw new NotFoundException("Skill assessment not found.");

        if (assessment.AcademyMemberProfile.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this assessment.");
        }

        return ToDto(assessment);
    }

    public async Task<List<SkillAssessmentListItemDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        var assessments = await _skillAssessmentRepository.GetByProfileAsync(profile.Id, cancellationToken);
        return assessments.Select(ToListItemDto).ToList();
    }

    private async Task<SkillAssessmentProviderInput> BuildInputAsync(
        Guid userId, AcademyMemberProfile profile, HeritageSkill heritageSkill, CancellationToken cancellationToken)
    {
        var currentLevel = profile.Skills.FirstOrDefault(s => s.HeritageSkillId == heritageSkill.Id)?.Level;

        var quizAttempts = await _quizRepository.GetMyAttemptsAsync(userId, cancellationToken);
        var examAttempts = await _examRepository.GetMyAttemptsAsync(userId, cancellationToken);
        var submissions = await _assignmentRepository.GetSubmissionsByStudentAsync(userId, cancellationToken);
        var enrollments = await _enrollmentRepository.GetByApprenticeAsync(userId, cancellationToken);
        var allSkills = await _heritageSkillRepository.GetAllAsync(true, cancellationToken);

        return new SkillAssessmentProviderInput
        {
            HeritageSkillId = heritageSkill.Id,
            HeritageSkillName = heritageSkill.Name,
            CurrentLevel = currentLevel,
            QuizPerformances = quizAttempts
                .Where(a => a.Status == AttemptStatus.Submitted && a.PercentageScore.HasValue)
                .Select(a => new PerformanceSignal
                {
                    Title = a.Quiz.Title,
                    PercentageScore = a.PercentageScore!.Value,
                    IsPassed = a.IsPassed,
                })
                .ToList(),
            ExamPerformances = examAttempts
                .Where(a => a.Status == AttemptStatus.Evaluated && a.PercentageScore.HasValue)
                .Select(a => new PerformanceSignal
                {
                    Title = a.Exam.Title,
                    PercentageScore = a.PercentageScore!.Value,
                    IsPassed = a.IsPassed,
                })
                .ToList(),
            AssignmentPerformances = submissions
                .Where(s => s.Status == SubmissionStatus.Graded && s.Score.HasValue && s.Assignment.MaxScore > 0)
                .Select(s => new PerformanceSignal
                {
                    Title = s.Assignment.Title,
                    PercentageScore = Math.Round(s.Score!.Value / (decimal)s.Assignment.MaxScore * 100m, 2),
                    IsPassed = null,
                })
                .ToList(),
            CompletedCourseCount = enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
            CandidateSkills = allSkills
                .Where(s => s.Id != heritageSkill.Id)
                .Select(s => new CandidateSkillInput { HeritageSkillId = s.Id, Name = s.Name })
                .ToList(),
        };
    }

    private static SkillAssessmentListItemDto ToListItemDto(SkillAssessmentEntity assessment) => new()
    {
        Id = assessment.Id,
        HeritageSkillId = assessment.HeritageSkillId,
        HeritageSkillName = assessment.HeritageSkill.Name,
        Level = assessment.Level.ToString(),
        Score = assessment.Score,
        AssessedAt = assessment.AssessedAt,
    };

    private static SkillAssessmentResultDto ToDto(SkillAssessmentEntity assessment) => new()
    {
        Id = assessment.Id,
        HeritageSkillId = assessment.HeritageSkillId,
        HeritageSkillName = assessment.HeritageSkill.Name,
        Level = assessment.Level.ToString(),
        Score = assessment.Score,
        Summary = assessment.Summary,
        Strengths = assessment.Insights
            .Where(i => i.Type == InsightType.Strength)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => i.Text)
            .ToList(),
        Weaknesses = assessment.Insights
            .Where(i => i.Type == InsightType.Weakness)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => i.Text)
            .ToList(),
        RecommendedSkills = assessment.RecommendedSkills
            .OrderBy(r => r.DisplayOrder)
            .Select(r => new RecommendedSkillDto
            {
                HeritageSkillId = r.HeritageSkillId,
                HeritageSkillName = r.HeritageSkill.Name,
                Reason = r.Reason,
            })
            .ToList(),
        AssessedAt = assessment.AssessedAt,
    };
}
